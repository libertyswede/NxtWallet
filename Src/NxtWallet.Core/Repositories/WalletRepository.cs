using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using NxtLib;
using NxtLib.Accounts;
using NxtLib.Local;
using NxtWallet.Core.Migrations.Model;
using NxtWallet.Repositories.Model;

namespace NxtWallet.Core.Repositories
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
        ulong LastLedgerEntryBlockId { get; }

        Task LoadAsync();
        Task UpdateBalanceAsync(string balance);
        Task UpdateNxtServerAsync(string newServerAddress);
        Task UpdateBackupCompletedAsync(bool completed);
        Task UpdateNotificationsEnabledAsync(bool newNotificationsEnabled);
        Task UpdateLastLedgerEntryBlockIdAsync(ulong blockId);
    }

    public class WalletRepository : IWalletRepository
    {
        private const string SecretPhraseKey = "secretPhrase";
        private const string BackupCompletedKey = "backupCompleted";
        private const string NxtServerKey = "nxtServer";
        private const string SleepTimeKey = "sleepTime";
        private const string BalanceKey = "balance";
        private const string NotificationsEnabledKey = "notificationsEnabled";
        private const string LastLedgerEntryBlockIdKey = "lastLedgerEntryBlockId";
        private const string ReadOnlyAccountKey = "readOnlyAccountKey";

        public Account NxtAccount { get; private set; }
        public AccountWithPublicKey NxtAccountWithPublicKey { get; private set; }
        public string SecretPhrase { get; private set; }
        public bool IsReadOnlyAccount { get; private set; }
        public string NxtServer { get; private set; }
        public bool BackupCompleted { get; private set; }
        public int SleepTime { get; private set; }
        public bool NotificationsEnabled { get; private set; }
        public ulong LastLedgerEntryBlockId { get; private set; }
        public string Balance { get; private set; }

        public async Task LoadAsync()
        {
            using (var context = new WalletContext())
            {
                CreateAndMigrateDb(context);

                var dbSettings = await context.Settings.ToListAsync();

                Balance = ReadOrGenerate(dbSettings, context, BalanceKey, () => "0.0");
                SecretPhrase = ReadOrGenerate(dbSettings, context, SecretPhraseKey, () => new LocalPasswordGenerator().GeneratePassword());
                var readOnlyAccount = ReadOrGenerate(dbSettings, context, ReadOnlyAccountKey, () => "");
                NxtServer = ReadOrGenerate(dbSettings, context, NxtServerKey, () => Constants.DefaultNxtUrl);
                SleepTime = ReadOrGenerate(dbSettings, context, SleepTimeKey, () => 30000);
                LastLedgerEntryBlockId = ReadOrGenerate(dbSettings, context, LastLedgerEntryBlockIdKey, () => Constants.GenesisBlockId);
                BackupCompleted = ReadOrGenerate(dbSettings, context, BackupCompletedKey, () => false);
                NotificationsEnabled = ReadOrGenerate(dbSettings, context, NotificationsEnabledKey, () => true);

                SetupAccounts(readOnlyAccount);

                await context.SaveChangesAsync();
            }
        }

        private void SetupAccounts(string readOnlyAccount)
        {
            IsReadOnlyAccount = !string.IsNullOrEmpty(readOnlyAccount);

            if (IsReadOnlyAccount)
            {
                NxtAccount = readOnlyAccount;
            }
            else
            {
                NxtAccountWithPublicKey = new LocalAccountService().GetAccount(AccountIdLocator.BySecretPhrase(SecretPhrase));
                NxtAccount = NxtAccountWithPublicKey;
            }
        }

        public async Task UpdateBalanceAsync(string balance)
        {
            await Update(BalanceKey, balance);
            Balance = balance;
        }

        public async Task UpdateNxtServerAsync(string newServerAddress)
        {
            await Update(NxtServerKey, newServerAddress);
            NxtServer = newServerAddress;
        }

        public async Task UpdateNotificationsEnabledAsync(bool newNotificationsEnabled)
        {
            await Update(NotificationsEnabledKey, newNotificationsEnabled);
            NotificationsEnabled = newNotificationsEnabled;
        }

        public async Task UpdateBackupCompletedAsync(bool completed)
        {
            await Update(BackupCompletedKey, completed);
            BackupCompleted = completed;
        }

        public async Task UpdateLastLedgerEntryBlockIdAsync(ulong blockId)
        {
            await Update(LastLedgerEntryBlockIdKey, blockId.ToString());
            LastLedgerEntryBlockId = blockId;
        }

        private static async Task Update<T>(string key, T value)
        {
            using (var context = new WalletContext())
            {
                var setting = context.Settings.Single(s => s.Key == key);
                setting.Value = value.ToString();
                await context.SaveChangesAsync();
            }
        }

        private static T ReadOrGenerate<T>(IEnumerable<SettingDto> dbSettings, WalletContext context, string key,
            Func<T> defaultValueAction) where T : IConvertible
        {
            var nullableValue = dbSettings.SingleOrDefault(s => s.Key.Equals(key))?.Value;
            if (nullableValue != null)
                return (T) Convert.ChangeType(nullableValue, typeof (T));

            var defaultValue = defaultValueAction.Invoke();
            context.Settings.Add(new SettingDto {Key = key, Value = defaultValue.ToString(CultureInfo.InvariantCulture)});
            return defaultValue;
        }

        private static void CreateAndMigrateDb(DbContext context)
        {
            context.Database.Migrate();
        }
    }
}
