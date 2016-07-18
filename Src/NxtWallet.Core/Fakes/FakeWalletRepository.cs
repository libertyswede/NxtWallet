using System;
using System.Threading.Tasks;
using NxtLib;
using NxtWallet.Core.Repositories;

namespace NxtWallet.Core.Fakes
{
    public class FakeWalletRepository : IWalletRepository
    {
        public Account NxtAccount { get; set; } 
        public AccountWithPublicKey NxtAccountWithPublicKey { get; set; }
        public string NxtServer { get; set; }
        public string SecretPhrase { get; set; }
        public bool IsReadOnlyAccount { get; set; }
        public string Balance { get; set; }
        public bool BackupCompleted { get; set; }
        public int SleepTime { get; set; }
        public bool NotificationsEnabled { get; set; }
        public ulong LastLedgerEntryBlockId { get; set; }

        public FakeWalletRepository()
        {
            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                UseDesignTimeData();
            }
        }

        private void UseDesignTimeData()
        {
            NxtAccount = new AccountWithPublicKey("NXT-5XAB-J4KK-5JKF-EA42X", "f91588343ba5a14e2a4960b2bfcf027e44e0d9337f683e0169d0e021714d3313");
            NxtAccountWithPublicKey = new AccountWithPublicKey("NXT-5XAB-J4KK-5JKF-EA42X", "f91588343ba5a14e2a4960b2bfcf027e44e0d9337f683e0169d0e021714d3313");
            Balance = "1100000000";
            BackupCompleted = false;
            SleepTime = 10000;
            NotificationsEnabled = true;
            LastLedgerEntryBlockId = 600000;
        }

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

        public Task UpdateBackupCompletedAsync(bool completed)
        {
            BackupCompleted = completed;
            return Task.CompletedTask;
        }

        public Task UpdateNotificationsEnabledAsync(bool newNotificationsEnabled)
        {
            NotificationsEnabled = newNotificationsEnabled;
            return Task.CompletedTask;
        }

        public Task UpdateLastLedgerEntryBlockIdAsync(ulong blockId)
        {
            LastLedgerEntryBlockId = blockId;
            return Task.CompletedTask;
        }

        public Task UpdateReadOnlyNxtAccountAsync(string nxtAccount)
        {
            NxtAccount = new AccountWithPublicKey("NXT-5XAB-J4KK-5JKF-EA42X", "f91588343ba5a14e2a4960b2bfcf027e44e0d9337f683e0169d0e021714d3313");
            return Task.CompletedTask;
        }

        public Task UpdateSecretPhraseAsync(string secretPhrase)
        {
            SecretPhrase = secretPhrase;
            return Task.CompletedTask;
        }
    }
}