using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxtWallet.Core.Models;
using NxtWallet.Repositories.Model;

namespace NxtWallet.Core.Fakes
{
    public class FakeTransactionRepository : ITransactionRepository
    {
        public List<Transaction> GetAllTransactions { get; set; }
        public List<Transaction> SaveTransaction { get; set; }
        public List<Transaction> UpdateTransactions { get; set; }
        public List<Transaction> RemoveTransaction { get; set; }
        public List<Transaction> SaveTransactions { get; set; }
        public bool HasOutgoingTransaction { get; set; }

        public FakeTransactionRepository()
        {
            GetAllTransactions = new List<Transaction>
            {
                new Transaction
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
                new Transaction
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
                new Transaction
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

            SaveTransaction = new List<Transaction>();
            UpdateTransactions = new List<Transaction>();
            RemoveTransaction = new List<Transaction>();
            SaveTransactions = new List<Transaction>();
        }

        public Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return Task.FromResult(GetAllTransactions.AsEnumerable());
        }

        public Task SaveTransactionAsync(Transaction transaction)
        {
            SaveTransaction.Add(transaction);
            return Task.CompletedTask;
        }

        public Task UpdateTransactionsAsync(IEnumerable<Transaction> transactionModels)
        {
            UpdateTransactions.AddRange(transactionModels);
            return Task.CompletedTask;
        }

        public Task RemoveTransactionAsync(Transaction transaction)
        {
            RemoveTransaction.Add(transaction);
            return Task.CompletedTask;
        }

        public Task SaveTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            SaveTransactions.AddRange(transactions);
            return Task.CompletedTask;
        }

        public Task<bool> HasOutgoingTransactionAsync()
        {
            return Task.FromResult(HasOutgoingTransaction);
        }
    }
}