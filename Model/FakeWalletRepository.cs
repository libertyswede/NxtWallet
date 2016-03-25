using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxtLib;

namespace NxtWallet.Model
{
    public class FakeWalletRepository : IWalletRepository
    {
        public AccountWithPublicKey NxtAccount { get; set; }
        public string NxtServer { get; set; }
        public string SecretPhrase { get; set; }
        public string Balance { get; set; }

        public Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ITransaction>> GetAllTransactionsAsync()
        {
            var transactions = new List<ITransaction>
            {
                new Transaction
                {
                    AccountFrom = "NXT-HMVV-XMBN-GYXK-22BKK",
                    AccountTo = "NXT-7BFH-DZVM-9G9Z-3ADJL",
                    Message = "Here ya go!",
                    NqtAmount = (long) (10.7*100000000),
                    NqtBalance = (long) (10.7*100000000),
                    NqtFeeAmount = 1*100000000,
                    Timestamp = DateTime.Now.AddDays(-1)
                },
                new Transaction
                {
                    AccountFrom = "NXT-7BFH-DZVM-9G9Z-3ADJL",
                    AccountTo = "NXT-HMVV-XMBN-GYXK-22BKK",
                    Message = "Keep the change",
                    NqtAmount = (long) (1.2*100000000),
                    NqtBalance = (long) (8.5*100000000),
                    NqtFeeAmount = 1*100000000,
                    Timestamp = DateTime.Now.AddDays(-1).AddMinutes(1)
                },
                new Transaction
                {
                    AccountFrom = "NXT-HMVV-XMBN-GYXK-22BKK",
                    AccountTo = "NXT-7BFH-DZVM-9G9Z-3ADJL",
                    Message = "Sorry, forgot some..",
                    NqtAmount = (long) (2.5*100000000),
                    NqtBalance = 11*100000000,
                    NqtFeeAmount = 1*100000000,
                    Timestamp = DateTime.Now.AddDays(-1).AddHours(1)
                }
            };

            return Task.FromResult(transactions.AsEnumerable());
        }

        public Task SaveTransactionAsync(ITransaction transaction)
        {
            return Task.CompletedTask;
        }

        public Task UpdateTransactionAsync(ITransaction transaction)
        {
            return Task.CompletedTask;
        }

        public Task SaveTransactionsAsync(IEnumerable<ITransaction> transactions)
        {
            return Task.CompletedTask;
        }

        public Task SaveBalanceAsync(string balance)
        {
            return Task.CompletedTask;
        }
    }
}