using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using NxtLib;
using NxtLib.Accounts;
using NxtLib.Local;

namespace NxtWallet.Model
{
    public class WalletRepository : IWalletRepository
    {
        private const string SecretPhraseKey = "secretPhrase";
        private const string BackupCompletedKey = "backupCompleted";
        private const string NxtServerKey = "nxtServer";
        private const string SleepTimeKey = "sleepTime";
        private const string BalanceKey = "balance";
        private const string NotificationsEnabledKey = "notificationsEnabled";
        private const string LastAssetTradeKey = "lastAssetTrade";

        public AccountWithPublicKey NxtAccount { get; private set; }
        public string NxtServer { get; private set; }
        public string SecretPhrase { get; private set; }
        public bool BackupCompleted { get; private set; }
        public int SleepTime { get; private set; }
        public bool NotificationsEnabled { get; private set; }
        public string Balance { get; private set; }
        public DateTime LastAssetTrade { get; private set; }

        public async Task LoadAsync()
        {
            using (var context = new WalletContext())
            {
                CreateAndMigrateDb(context);

                var dbSettings = await context.Settings.ToListAsync();

                Balance = ReadOrGenerate(dbSettings, context, BalanceKey, () => "0.0");
                SecretPhrase = ReadOrGenerate(dbSettings, context, SecretPhraseKey, () => new LocalPasswordGenerator().GeneratePassword());
                NxtServer = ReadOrGenerate(dbSettings, context, NxtServerKey, () => Constants.DefaultNxtUrl);
                SleepTime = ReadOrGenerate(dbSettings, context, SleepTimeKey, () => 30000);
                BackupCompleted = ReadOrGenerate(dbSettings, context, BackupCompletedKey, () => false);
                NotificationsEnabled = ReadOrGenerate(dbSettings, context, NotificationsEnabledKey, () => true);
                LastAssetTrade = ReadOrGenerateDateTime(dbSettings, context, LastAssetTradeKey, () => new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc));

                NxtAccount = new LocalAccountService().GetAccount(AccountIdLocator.BySecretPhrase(SecretPhrase));
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateLastAssetTrade(DateTime newTimestamp)
        {
            await Update(LastAssetTradeKey, newTimestamp.ToString("O"));
            LastAssetTrade = newTimestamp;
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

        public async Task UpdateBackupCompleted(bool completed)
        {
            await Update(BackupCompletedKey, completed);
            BackupCompleted = completed;
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

        private static DateTime ReadOrGenerateDateTime(IEnumerable<SettingDto> dbSettings, WalletContext context,
            string key, Func<DateTime> defaultValueAction)
        {
            var value = ReadOrGenerate(dbSettings, context, key, () => defaultValueAction.Invoke().ToString("O"));
            return DateTime.ParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        private static void CreateAndMigrateDb(DbContext context)
        {
            context.Database.Migrate();
        }
    }
}
