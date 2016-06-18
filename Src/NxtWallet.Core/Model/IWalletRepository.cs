using System;
using System.Threading.Tasks;
using NxtLib;

namespace NxtWallet.Core.Model
{
    public interface IWalletRepository
    {
        Account NxtAccount { get; }
        AccountWithPublicKey NxtAccountWithPublicKey { get; }
        string SecretPhrase { get; }
        bool IsReadOnlyAccount { get; }
        string NxtServer { get; }
        string Balance { get; }
        bool BackupCompleted { get; }
        int SleepTime { get; }
        bool NotificationsEnabled { get; }
        ulong LastBalanceMatchBlockId { get; }
        DateTime LastAssetTrade { get; }
        DateTime LastCurrencyExchange { get; }

        Task LoadAsync();
        Task UpdateBalanceAsync(string balance);
        Task UpdateNxtServerAsync(string newServerAddress);
        Task UpdateBackupCompleted(bool completed);
        Task UpdateNotificationsEnabledAsync(bool newNotificationsEnabled);
        Task UpdateLastAssetTrade(DateTime newTimestamp);
        Task UpdateLastCurrencyExchange(DateTime newTimestamp);
        Task UpdateLastBalanceMatchBlockIdAsync(ulong blockId);
    }
}