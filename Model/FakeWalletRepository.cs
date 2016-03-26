using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxtLib;

namespace NxtWallet.Model
{
    public class FakeWalletRepository : IWalletRepository
    {
        public AccountWithPublicKey NxtAccount { get; set; } = new AccountWithPublicKey("NXT-5XAB-J4KK-5JKF-EA42X", "f91588343ba5a14e2a4960b2bfcf027e44e0d9337f683e0169d0e021714d3313");
        public string NxtServer { get; set; }
        public string SecretPhrase { get; set; }
        public string Balance { get; set; } = "1100000000";

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
                    NxtId = 1,
                    AccountFrom = "NXT-HMVV-XMBN-GYXK-22BKK",
                    AccountTo = NxtAccount.AccountRs,
                    Message = "Here ya go!",
                    NqtAmount = (long) (10.7M * 100000000),
                    NqtBalance = (long) (10.7M * 100000000),
                    NqtFeeAmount = 1*100000000,
                    Timestamp = DateTime.Now.AddDays(-1)
                },
                new Transaction
                {
                    NxtId = 2,
                    AccountFrom = NxtAccount.AccountRs,
                    AccountTo = "NXT-HMVV-XMBN-GYXK-22BKK",
                    Message = "Keep the change",
                    NqtAmount = (long) (1.2M * 100000000),
                    NqtBalance = (long) (8.5M * 100000000),
                    NqtFeeAmount = 1*100000000,
                    Timestamp = DateTime.Now.AddDays(-1).AddMinutes(1)
                },
                new Transaction
                {
                    NxtId = 3,
                    AccountFrom = "NXT-HMVV-XMBN-GYXK-22BKK",
                    AccountTo = NxtAccount.AccountRs,
                    Message = "Sorry, forgot some..",
                    NqtAmount = (long) (2.5M * 100000000),
                    NqtBalance = 11 * 100000000,
                    NqtFeeAmount = 1 * 100000000,
                    Timestamp = DateTime.Now.AddDays(-1).AddHours(1)
                }
            };

            return Task.FromResult(transactions.AsEnumerable());
        }

        public Task SaveTransactionAsync(ITransaction transaction)
        {
            return Task.CompletedTask;
        }

        public Task UpdateTransactionsAsync(IEnumerable<ITransaction> transactions)
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

        public Task<ITransaction> GetLatestTransactionAsync()
        {
            return Task.FromResult(GetAllTransactionsAsync().Result.Last());
        }
    }
}