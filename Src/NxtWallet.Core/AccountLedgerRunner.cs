using NxtWallet.Core.Models;
using NxtWallet.Core.Repositories;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NxtWallet.Core
{
    public delegate void AccountLedgerHandler(IAccountLedgerRunner sender, LedgerEntry ledgerEntry);
    public delegate void BalanceHandler(IAccountLedgerRunner sender, string balance);

    public interface IAccountLedgerRunner
    {
        Task Run(CancellationToken token);

        event AccountLedgerHandler AccountLedgerConfirmationUpdated;
        event AccountLedgerHandler AccountLedgerBalanceUpdated;
        event AccountLedgerHandler AccountLedgerAdded;
        event BalanceHandler BalanceUpdated;
    }

    public class AccountLedgerRunner : IAccountLedgerRunner
    {

        public event BalanceHandler BalanceUpdated;
        public event AccountLedgerHandler AccountLedgerAdded;
        public event AccountLedgerHandler AccountLedgerBalanceUpdated;
        public event AccountLedgerHandler AccountLedgerConfirmationUpdated;
        
        private readonly IWalletRepository _walletRepository;
        private readonly INxtServer _nxtServer;

        public AccountLedgerRunner(IWalletRepository walletRepository, INxtServer nxtServer)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;
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
            // Pseudo code below, replace with real stuff...

            // Fork check...
            // Get last known block from DB.
            // Get blockchain status from NRS.
            // If fork, roll back local db 10 blocks, if still on fork, roll back to genesis.
            // Things to roll back: ledger events, balance, unconfirmed transactions
            // Fire events (removed)

            // Fetch stuff...
            // Get balance from DB.
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
            AccountLedgerConfirmationUpdated?.Invoke(this, ledgerEntry);
        }

        protected virtual void OnLedgerEntryBalanceUpdated(LedgerEntry ledgerEntry)
        {
            AccountLedgerBalanceUpdated?.Invoke(this, ledgerEntry);
        }

        protected virtual void OnLedgerEntryAdded
            (LedgerEntry ledgerEntry)
        {
            AccountLedgerAdded?.Invoke(this, ledgerEntry);
        }

        protected virtual void OnBalanceUpdated(string balance)
        {
            BalanceUpdated?.Invoke(this, balance);
        }
    }
}
