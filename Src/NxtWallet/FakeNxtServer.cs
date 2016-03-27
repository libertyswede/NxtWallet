using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtLib;
using NxtWallet.Model;
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

        public Task<IEnumerable<ITransaction>> GetTransactionsAsync(DateTime lastTimestamp)
        {
            return Task.FromResult(new List<ITransaction>().AsEnumerable());
        }

        public Task<IEnumerable<ITransaction>> GetTransactionsAsync()
        {
            return GetTransactionsAsync(DateTime.UtcNow);
        }

        public Task<ITransaction> SendMoneyAsync(Account recipient, Amount amount, string message)
        {
            return Task.FromResult((ITransaction)new Transaction());
        }
    }
}