using System;
using System.Threading.Tasks;
using NxtLib;

namespace NxtWallet.Model
{
    public interface IWalletRepository
    {
        AccountWithPublicKey NxtAccount { get; }
        string NxtServer { get; }
        string SecretPhrase { get; }
        string Balance { get; }
        bool BackupCompleted { get; }
        int SleepTime { get; }
        bool NotificationsEnabled { get; }
        ulong LastBalanceMatchBlockId { get; }
        DateTime LastAssetTrade { get; }

        Task LoadAsync();
        Task UpdateBalanceAsync(string balance);
        Task UpdateNxtServerAsync(string newServerAddress);
        Task UpdateBackupCompleted(bool completed);
        Task UpdateNotificationsEnabledAsync(bool newNotificationsEnabled);
        Task UpdateLastAssetTrade(DateTime newTimestamp);
        Task UpdateLastBalanceMatchBlockIdAsync(ulong blockId);
    }
}