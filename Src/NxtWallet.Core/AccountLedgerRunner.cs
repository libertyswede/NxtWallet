using NxtWallet.Core.Models;
using NxtWallet.Core.Repositories;
using System.Threading;
using System.Threading.Tasks;
using System;
using NxtLib;

namespace NxtWallet.Core
{
    public delegate void AccountLedgerHandler(IAccountLedgerRunner sender, LedgerEntry ledgerEntry);
    public delegate void BalanceHandler(IAccountLedgerRunner sender, string balance);

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
                //await Task.Delay(_walletRepository.SleepTime, token);
                await Task.Delay(int.MaxValue, token);
            }
        }

        public async Task TryCheckAllLedgerEntries()
        {
            // Fork check...
            // Get last known block from DB.
            // Get blockchain status from NRS.
            // If fork, roll back local db 10 blocks, if still on fork, roll back to genesis.
            // Things to roll back: ledger events, balance, unconfirmed transactions
            // Fire events (removed)
            try
            {
                var blockchainStatus = await _nxtServer.GetBlockchainStatusAsync();
                var lastKnownBlock = await _nxtServer.GetBlockAsync(_walletRepository.LastLedgerEntryBlockId);
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    if ((e as NxtException)?.Message == "Unknown block")
                    {
                        // Handle fork
                        return true;
                    }
                    return false;
                });
            }
            
            // Fetch stuff...
            // Get previously unconfirmed transactions from DB.
            // Get new ledger entries since last known block from NRS
            // Get unconfirmed balance from NRS
            // Get unconfirmed transactions from NRS, non-phased sendMoney only.

            // Update transaction statuses..
            // If previously unconfirmed transactions are now account ledger events
            //   Remove them from unconfirmed transactions table in DB.
            //   Add to AccountLedgerConfirmationUpdated
            // If previously unconfirmed transaction is not to be found in account ledger events
            //   Remove them from unconfirmed transactions table in DB.
            //   Add to AccountLedgerRemoved

            // Check stuff...
            // If NRS.unconfirmedBalance != last ledger entry balance + sum(unconfirmed transaction amounts)
            //   Log error, throw exception!
            // Save stuff to local DB.
            // Fire event(s).

            var ledgerEntries = await _nxtServer.GetAccountLedgerEntriesAsync();
            ledgerEntries.ForEach(e => OnLedgerEntryAdded(e));
        }

        protected virtual void OnLedgerEntryConfirmationUpdated(LedgerEntry ledgerEntry)
        {
            LedgerEntryConfirmationUpdated?.Invoke(this, ledgerEntry);
        }

        protected virtual void OnLedgerEntryBalanceUpdated(LedgerEntry ledgerEntry)
        {
            LedgerEntryRemoved?.Invoke(this, ledgerEntry);
        }

        protected virtual void OnLedgerEntryAdded
            (LedgerEntry ledgerEntry)
        {
            LedgerEntryAdded?.Invoke(this, ledgerEntry);
        }

        protected virtual void OnBalanceUpdated(string balance)
        {
            BalanceUpdated?.Invoke(this, balance);
        }
    }
}
