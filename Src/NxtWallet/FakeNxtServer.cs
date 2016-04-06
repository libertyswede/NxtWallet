using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtLib;
using NxtWallet.Model;
using NxtWallet.ViewModel.Model;
using Transaction = NxtWallet.Model.Transaction;

namespace NxtWallet
{
    public class FakeNxtServer : ObservableObject, INxtServer
    {
        private bool _isOnline = true;

        public bool IsOnline
        {
            get { return _isOnline; }
            set { Set(ref _isOnline, value); }
        }

        public Task<Result<string>> GetBalanceAsync()
        {
            return Task.FromResult(new Result<string>("11.00"));
        }

        public Task<IEnumerable<TransactionModel>> GetTransactionsAsync(DateTime lastTimestamp)
        {
            return Task.FromResult(new List<TransactionModel>().AsEnumerable());
        }

        public Task<IEnumerable<TransactionModel>> GetTransactionsAsync()
        {
            return GetTransactionsAsync(DateTime.UtcNow);
        }

        public Task<Result<TransactionModel>> SendMoneyAsync(Account recipient, Amount amount, string message)
        {
            return Task.FromResult(new Result<TransactionModel>());
        }

        public void UpdateNxtServer(string newServerAddress)
        {
        }
    }
}