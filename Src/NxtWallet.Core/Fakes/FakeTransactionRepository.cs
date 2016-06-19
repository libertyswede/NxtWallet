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
        public List<Transaction> Transactions { get; set; }

        private IWalletRepository _walletRepository;

        public FakeTransactionRepository(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
            Transactions = new List<Transaction>();

            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                UseDesignTimeData();
            }
        }

        public FakeTransactionRepository() : this(null)
        {
        }

        public Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return Task.FromResult(Transactions.OrderByDescending(t => t.Timestamp).AsEnumerable());
        }

        public Task SaveTransactionAsync(Transaction transaction)
        {
            if (transaction.Id == 0)
                transaction.Id = Transactions.Any() ? Transactions.Max(t => t.Id) + 1 : 1;
            Transactions.Add(transaction);
            return Task.CompletedTask;
        }

        public Task UpdateTransactionsAsync(IEnumerable<Transaction> transactionModels)
        {
            foreach (var updatedTransaction in transactionModels)
            {
                var storedTransaction = Transactions.Single(t => t.Id == updatedTransaction.Id);
                Transactions.Remove(storedTransaction);
                Transactions.Add(updatedTransaction);
            }
            return Task.CompletedTask;
        }

        public Task RemoveTransactionAsync(Transaction transaction)
        {
            var storedTransaction = Transactions.Single(t => t.Id == transaction.Id);
            Transactions.Remove(storedTransaction);
            return Task.CompletedTask;
        }

        public async Task SaveTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                await SaveTransactionAsync(transaction);
            }
        }

        public Task<bool> HasOutgoingTransactionAsync()
        {
            if (_walletRepository != null)
            {
                var hasOutgoingTransaction = Transactions.Any(t => t.AccountFrom == _walletRepository.NxtAccount.AccountRs);
                return Task.FromResult(hasOutgoingTransaction);
            }
            return Task.FromResult(false);
        }

        private void UseDesignTimeData()
        {
            Transactions = new List<Transaction>
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
        }
    }
}