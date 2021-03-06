﻿using NxtWallet.Core.Models;
using NxtWallet.Core.Repositories;
using System.Threading;
using System.Threading.Tasks;
using System;
using NxtLib;
using NxtLib.ServerInfo;
using System.Linq;
using System.Collections.Generic;
using NxtLib.Local;
using GalaSoft.MvvmLight.Messaging;
using System.Net.Http;

namespace NxtWallet.Core
{
    public interface IAccountLedgerRunner
    {
        Task Run(CancellationToken token);
    }

    public class AccountLedgerRunner : IAccountLedgerRunner
    {
        private readonly IWalletRepository _walletRepository;
        private readonly INxtServer _nxtServer;
        private readonly IAccountLedgerRepository _accountLedgerRepository;

        private CancellationTokenSource _cancellationTokenSource;

        public AccountLedgerRunner(IWalletRepository walletRepository, INxtServer nxtServer, 
            IAccountLedgerRepository accountLedgerRepository)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;
            _accountLedgerRepository = accountLedgerRepository;

            Messenger.Default.Register<SecretPhraseResetMessage>(this, (message) => _cancellationTokenSource.Cancel());
        }

        public async Task Run(CancellationToken globalToken)
        {
            var mergedToken = CreateMergedCancellationToken(globalToken);

            while (!globalToken.IsCancellationRequested)
            {
                try
                {
                    await TryCheckAllLedgerEntries(mergedToken);
                }
                catch (HttpRequestException)
                {
                    // Ignore and try again later
                }
                try
                {
                    await Task.Delay(_walletRepository.SleepTime, mergedToken);
                }
                catch (OperationCanceledException)
                {
                    mergedToken = CreateMergedCancellationToken(globalToken);
                }
            }
        }
        private CancellationToken CreateMergedCancellationToken(CancellationToken globalToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var mergedToken = CancellationTokenSource.CreateLinkedTokenSource(globalToken, _cancellationTokenSource.Token).Token;
            return mergedToken;
        }

        public async Task TryCheckAllLedgerEntries(CancellationToken cancellationToken)
        {
            var syncBlockInfo = await SyncToLastCommonBlock();
            var blockchainStatus = syncBlockInfo.Item1;
            var lastKnownBlock = syncBlockInfo.Item2;

            var knownUnconfirmedEntries = await _accountLedgerRepository.GetUnconfirmedLedgerEntriesAsync();
            var nrsUnconfirmedLedgerEntries = await _nxtServer.GetUnconfirmedAccountLedgerEntriesAsync();
            var nrsConfirmedLedgerEntries = await _nxtServer.GetAccountLedgerEntriesAsync(lastKnownBlock.Timestamp);
            var nrsLedgerEntries = nrsUnconfirmedLedgerEntries.Union(nrsConfirmedLedgerEntries).ToList();
            var nrsUnconfirmedBalance = await _nxtServer.GetUnconfirmedNqtBalanceAsync();
            var updatedLedgerEntries = GetConfirmedEntries(knownUnconfirmedEntries, nrsConfirmedLedgerEntries).ToList();
            var deletedLedgerEntries = GetLostUnconfirmedEntries(knownUnconfirmedEntries, nrsLedgerEntries).ToList();

            // If NRS.unconfirmedBalance != last ledger entry balance + sum(unconfirmed transaction amounts)
            //   Log error, throw exception!

            cancellationToken.ThrowIfCancellationRequested();
            
            await _walletRepository.UpdateLastLedgerEntryBlockIdAsync(blockchainStatus.LastBlockId);
            if (_walletRepository.NqtBalance != nrsUnconfirmedBalance)
            {
                await _walletRepository.UpdateBalanceAsync(nrsUnconfirmedBalance);
            }
            await _accountLedgerRepository.AddLedgerEntriesAsync(nrsConfirmedLedgerEntries);
            await _accountLedgerRepository.UpdateLedgerEntriesAsync(updatedLedgerEntries);
            await _accountLedgerRepository.RemoveLedgerEntriesAsync(deletedLedgerEntries);

            Messenger.Default.Send(new BalanceUpdatedMessage(nrsUnconfirmedBalance));
            deletedLedgerEntries.ForEach(e => Messenger.Default.Send(new LedgerEntryMessage(e, LedgerEntryMessageAction.Removed)));
            nrsConfirmedLedgerEntries.ForEach(e => Messenger.Default.Send(new LedgerEntryMessage(e, LedgerEntryMessageAction.Added)));
            updatedLedgerEntries.ForEach(e => Messenger.Default.Send(new LedgerEntryMessage(e, LedgerEntryMessageAction.ConfirmationUpdated)));
        }

        private async Task<Tuple<BlockchainStatus, Block<ulong>>> SyncToLastCommonBlock()
        {
            Block<ulong> lastKnownBlock = null;
            BlockchainStatus blockchainStatus = null;

            while (lastKnownBlock == null)
            {
                try
                {
                    blockchainStatus = await _nxtServer.GetBlockchainStatusAsync();
                    lastKnownBlock = await _nxtServer.GetBlockAsync(_walletRepository.LastLedgerEntryBlockId);
                }
                catch (NxtException e)
                {
                    if (e.Message != "Unknown block")
                        throw;
                    await RollbackToPreviousHeight();
                }
            }
            return new Tuple<BlockchainStatus, Block<ulong>>(blockchainStatus, lastKnownBlock);
        }

        private async Task RollbackToPreviousHeight()
        {
            var ledgerEntries = await _accountLedgerRepository.GetLedgerEntriesOnLastBlockAsync();

            if (!ledgerEntries.Any())
            {
                await _walletRepository.UpdateLastLedgerEntryBlockIdAsync(Constants.GenesisBlockId);
                return;
            }

            var lastLedgerEntryBlockId = ledgerEntries.First().BlockId.Value;
            try
            {
                var block = await _nxtServer.GetBlockAsync(lastLedgerEntryBlockId);
            }
            catch (AggregateException ae)
            {
                ae.Handle(e => (e as NxtException)?.Message == "Unknown block");
            }
            await _walletRepository.UpdateLastLedgerEntryBlockIdAsync(lastLedgerEntryBlockId);
            await _accountLedgerRepository.RemoveLedgerEntriesOnBlockAsync(lastLedgerEntryBlockId);
            ledgerEntries.ForEach(e => Messenger.Default.Send(new LedgerEntryMessage(e, LedgerEntryMessageAction.Removed)));
        }

        private static IEnumerable<LedgerEntry> GetConfirmedEntries(List<LedgerEntry> knownUnconfirmedEntries, List<LedgerEntry> newLedgerEntries)
        {
            for (int i = 0; i < knownUnconfirmedEntries.Count; i++)
            {
                var unconfirmedEntry = knownUnconfirmedEntries[i];
                var confirmedEntry = newLedgerEntries.SingleOrDefault(e => e.TransactionId == unconfirmedEntry.TransactionId);
                if (confirmedEntry == null)
                {
                    continue;
                }
                confirmedEntry.Id = unconfirmedEntry.Id;
                newLedgerEntries.Remove(confirmedEntry);
                yield return confirmedEntry;
                knownUnconfirmedEntries.RemoveAt(i--);
            }
        }

        private static IEnumerable<LedgerEntry> GetLostUnconfirmedEntries(List<LedgerEntry> knownUnconfirmedEntries,
            List<LedgerEntry> nrsUnconfirmedLedgerEntries)
        {
            return knownUnconfirmedEntries.Where(known => nrsUnconfirmedLedgerEntries.All(nrs => nrs.TransactionId != known.TransactionId));
        }
    }
}
