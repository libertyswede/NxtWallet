using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public class FakeTransactionRepository : ITransactionRepository
    {
        public Task<IEnumerable<TransactionModel>> GetAllTransactionsAsync()
        {
            var transactions = new List<TransactionModel>
            {
                new TransactionModel
                {
                    NxtId = 1,
                    AccountFrom = "NXT-HMVV-XMBN-GYXK-22BKK",
                    AccountTo = "NXT-5XAB-J4KK-5JKF-EA42X",
                    Message = "Here ya go!",
                    NqtAmount = (long) (10.7M * 100000000),
                    NqtBalance = (long) (10.7M * 100000000),
                    NqtFee = 1*100000000,
                    Timestamp = DateTime.Now.AddDays(-1)
                },
                new TransactionModel
                {
                    NxtId = 2,
                    AccountFrom = "NXT-5XAB-J4KK-5JKF-EA42X",
                    AccountTo = "NXT-HMVV-XMBN-GYXK-22BKK",
                    Message = "Keep the change",
                    NqtAmount = (long) (1.2M * 100000000),
                    NqtBalance = (long) (8.5M * 100000000),
                    NqtFee = 1*100000000,
                    Timestamp = DateTime.Now.AddDays(-1).AddMinutes(1)
                },
                new TransactionModel
                {
                    NxtId = 3,
                    AccountFrom = "NXT-HMVV-XMBN-GYXK-22BKK",
                    AccountTo = "NXT-5XAB-J4KK-5JKF-EA42X",
                    Message = "Sorry, forgot some..",
                    NqtAmount = (long) (2.5M * 100000000),
                    NqtBalance = 11 * 100000000,
                    NqtFee = 1*100000000,
                    Timestamp = DateTime.Now.AddDays(-1).AddHours(1)
                }
            };

            return Task.FromResult(transactions.AsEnumerable());
        }

        public Task SaveTransactionAsync(TransactionModel transactionModel)
        {
            return Task.CompletedTask;
        }

        public Task UpdateTransactionsAsync(IEnumerable<TransactionModel> transactionModels)
        {
            return Task.CompletedTask;
        }

        public Task SaveTransactionsAsync(IEnumerable<TransactionModel> transactionModels)
        {
            return Task.CompletedTask;
        }

        public Task<TransactionModel> GetLatestTransactionAsync()
        {
            return Task.FromResult(GetAllTransactionsAsync().Result.Last());
        }
    }
}