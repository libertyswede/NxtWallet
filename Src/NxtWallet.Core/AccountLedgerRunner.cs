using NxtWallet.Core.Models;
using NxtWallet.Core.Repositories;
using System.Threading;
using System.Threading.Tasks;
using System;
using NxtLib;
using NxtLib.ServerInfo;
using System.Linq;
using System.Collections.Generic;
using NxtLib.Local;

namespace NxtWallet.Core
{
    public delegate void AccountLedgerHandler(IAccountLedgerRunner sender, LedgerEntry ledgerEntry);
    public delegate void BalanceHandler(IAccountLedgerRunner sender, long nqtBalance);

    public interface IAccountLedgerRunner
    {
        Task Run(CancellationToken token);

        event AccountLedgerHandler LedgerEntryAdded;
        event AccountLedgerHandler LedgerEntryRemoved;
        event AccountLedgerHandler LedgerEntryConfirmationUpdated;
        event BalanceHandler BalanceUpdated;
    }

    public class AccountLedgerRunner : IAccountLedgerRunner
    {
        public event AccountLedgerHandler LedgerEntryAdded;
        public event AccountLedgerHandler LedgerEntryRemoved;
        public event AccountLedgerHandler LedgerEntryConfirmationUpdated;
        public event BalanceHandler BalanceUpdated;

        private readonly IWalletRepository _walletRepository;
        private readonly INxtServer _nxtServer;
        private readonly IAccountLedgerRepository _accountLedgerRepository;

        public AccountLedgerRunner(IWalletRepository walletRepository, INxtServer nxtServer, 
            IAccountLedgerRepository accountLedgerRepository)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;
            _accountLedgerRepository = accountLedgerRepository;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await TryCheckAllLedgerEntries();
                await Task.Delay(_walletRepository.SleepTime, token);
            }
        }

        public async Task TryCheckAllLedgerEntries()
        {
            var syncBlockInfo = await SyncToLastCommonBlock();
            var blockchainStatus = syncBlockInfo.Item1;
            var lastKnownBlock = syncBlockInfo.Item2;

            var knownUnconfirmedEntries = await _accountLedgerRepository.GetUnconfirmedLedgerEntriesAsync();
            var newLedgerEntries = await _nxtServer.GetAccountLedgerEntriesAsync(lastKnownBlock.Timestamp);
            var unconfirmedBalance = await _nxtServer.GetUnconfirmedNqtBalanceAsync();
            var updatedLedgerEntries = new List<LedgerEntry>();
            CheckForConfirmedEntries(knownUnconfirmedEntries, newLedgerEntries, updatedLedgerEntries);
            
            // If NRS.unconfirmedBalance != last ledger entry balance + sum(unconfirmed transaction amounts)
            //   Log error, throw exception!

            await _walletRepository.UpdateLastLedgerEntryBlockIdAsync(blockchainStatus.LastBlockId);
            if (_walletRepository.NqtBalance != unconfirmedBalance)
            {
                await _walletRepository.UpdateBalanceAsync(unconfirmedBalance);
                OnBalanceUpdated(unconfirmedBalance);
            }
            await _accountLedgerRepository.AddLedgerEntriesAsync(newLedgerEntries);
            newLedgerEntries.ForEach(e => OnLedgerEntryAdded(e));

            await _accountLedgerRepository.UpdateLedgerEntriesAsync(updatedLedgerEntries);
            updatedLedgerEntries.ForEach(e => OnLedgerEntryConfirmationUpdated(e));
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
            ledgerEntries.ForEach(e => OnLedgerEntryRemoved(e));
        }

        private static void CheckForConfirmedEntries(List<LedgerEntry> knownUnconfirmedEntries, List<LedgerEntry> newLedgerEntries, List<LedgerEntry> updatedLedgerEntries)
        {
            foreach (var unconfirmedEntry in knownUnconfirmedEntries)
            {
                var confirmedEntry = newLedgerEntries.SingleOrDefault(e => e.TransactionId == unconfirmedEntry.TransactionId);
                if (confirmedEntry == null)
                {
                    continue;
                }
                confirmedEntry.Id = unconfirmedEntry.Id;
                newLedgerEntries.Remove(confirmedEntry);
                updatedLedgerEntries.Add(confirmedEntry);
            }
        }

        private void OnLedgerEntryAdded(LedgerEntry ledgerEntry)
        {
            LedgerEntryAdded?.Invoke(this, ledgerEntry);
        }

        private void OnLedgerEntryConfirmationUpdated(LedgerEntry ledgerEntry)
        {
            LedgerEntryConfirmationUpdated?.Invoke(this, ledgerEntry);
        }

        private void OnLedgerEntryRemoved(LedgerEntry ledgerEntry)
        {
            LedgerEntryRemoved?.Invoke(this, ledgerEntry);
        }

        private void OnBalanceUpdated(long nqtBalance)
        {
            BalanceUpdated?.Invoke(this, nqtBalance);
        }
    }
}
