using NxtWallet.Core.Models;
using NxtWallet.Repositories.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NxtWallet.Core
{
    public delegate void AccountLedgerHandler(IAccountLedgerRunner sender, LedgerEntry ledgerEntry);
    public delegate void BalanceHandler(IAccountLedgerRunner sender, string balance);

    public interface IAccountLedgerRunner
    {
        Task Run(CancellationToken token);

        event AccountLedgerHandler TransactionConfirmationUpdated;
        event AccountLedgerHandler TransactionBalanceUpdated;
        event AccountLedgerHandler TransactionAdded;
        event BalanceHandler BalanceUpdated;
    }

    public class AccountLedgerRunner : IAccountLedgerRunner
    {

        public event BalanceHandler BalanceUpdated;
        public event AccountLedgerHandler TransactionAdded;
        public event AccountLedgerHandler TransactionBalanceUpdated;
        public event AccountLedgerHandler TransactionConfirmationUpdated;
        
        private readonly IWalletRepository _walletRepository;
        private readonly INxtServer _nxtServer;

        private HashSet<LedgerEntry> _updatedLedgerEntries;

        public AccountLedgerRunner(IWalletRepository walletRepository, INxtServer nxtServer)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await TryCheckAllTransactions();
                await Task.Delay(_walletRepository.SleepTime, token);
            }
        }

        public async Task TryCheckAllTransactions()
        {
            var accountLedger = _nxtServer.GetAccountLedgerEntriesAsync();

        }

        protected virtual void OnTransactionConfirmationUpdated(LedgerEntry ledgerEntry)
        {
            TransactionConfirmationUpdated?.Invoke(this, ledgerEntry);
        }

        protected virtual void OnTransactionBalanceUpdated(LedgerEntry ledgerEntry)
        {
            TransactionBalanceUpdated?.Invoke(this, ledgerEntry);
        }

        protected virtual void OnTransactionAdded(LedgerEntry ledgerEntry)
        {
            TransactionAdded?.Invoke(this, ledgerEntry);
        }

        protected virtual void OnBalanceUpdated(string balance)
        {
            BalanceUpdated?.Invoke(this, balance);
        }
    }
}
