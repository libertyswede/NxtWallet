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
                await TryCheckAllTransactions();
                //await Task.Delay(_walletRepository.SleepTime, token);
                await Task.Delay(int.MaxValue, token);
            }
        }

        public async Task TryCheckAllTransactions()
        {
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
