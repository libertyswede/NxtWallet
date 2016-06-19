using System;
using System.Threading.Tasks;
using NxtLib;
using NxtWallet.Repositories.Model;

namespace NxtWallet.Core.Fakes
{
    public class FakeWalletRepository : IWalletRepository
    {
        public Account NxtAccount { get; set; } = new AccountWithPublicKey("NXT-5XAB-J4KK-5JKF-EA42X", "f91588343ba5a14e2a4960b2bfcf027e44e0d9337f683e0169d0e021714d3313");
        public AccountWithPublicKey NxtAccountWithPublicKey { get; set; } = new AccountWithPublicKey("NXT-5XAB-J4KK-5JKF-EA42X", "f91588343ba5a14e2a4960b2bfcf027e44e0d9337f683e0169d0e021714d3313");
        public string NxtServer { get; set; }
        public string SecretPhrase { get; set; }
        public bool IsReadOnlyAccount { get; set; }
        public string Balance { get; set; } = "1100000000";
        public bool BackupCompleted { get; set; } = false;
        public int SleepTime { get; set; } = 10000;
        public bool NotificationsEnabled { get; set; } = true;
        public ulong LastBalanceMatchBlockId { get; set; } = 600000;
        public DateTime LastAssetTrade { get; set; } = DateTime.UtcNow;
        public DateTime LastCurrencyExchange { get; set; } = DateTime.Now;

        public Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        public Task UpdateBalanceAsync(string balance)
        {
            Balance = balance;
            return Task.CompletedTask;
        }

        public Task UpdateNxtServerAsync(string newServerAddress)
        {
            NxtServer = newServerAddress;
            return Task.CompletedTask;
        }

        public Task UpdateBackupCompleted(bool completed)
        {
            BackupCompleted = completed;
            return Task.CompletedTask;
        }

        public Task UpdateNotificationsEnabledAsync(bool newNotificationsEnabled)
        {
            NotificationsEnabled = newNotificationsEnabled;
            return Task.CompletedTask;
        }

        public Task UpdateLastAssetTrade(DateTime newTimestamp)
        {
            LastAssetTrade = newTimestamp;
            return Task.CompletedTask;
        }

        public Task UpdateLastCurrencyExchange(DateTime newTimestamp)
        {
            LastCurrencyExchange = newTimestamp;
            return Task.CompletedTask;
        }

        public Task UpdateLastBalanceMatchBlockIdAsync(ulong blockId)
        {
            LastBalanceMatchBlockId = blockId;
            return Task.CompletedTask;
        }
    }
}