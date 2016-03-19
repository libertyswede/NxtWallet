using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using NxtLib;
using NxtLib.Accounts;
using NxtLib.Local;
using NxtWallet.Model;
using Transaction = NxtWallet.Model.Transaction;

namespace NxtWallet
{
    public class WalletSettings
    {
        public const string SecretPhraseKey = "secretPhrase";
        public const string NxtServerKey = "nxtServer";
        public const string BalanceKey = "balance";

        public static AccountWithPublicKey NxtAccount { get; private set; }
        public static string NxtServer { get; private set; }
        public static string SecretPhrase { get; private set; }
        public static string Balance { get; private set; }

        public static async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            using (var context = new WalletContext())
            {
                var transactions = await context.Transactions.ToListAsync();
                return transactions;
            }
        }

        public static async Task SaveTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            using (var context = new WalletContext())
            {
                var existingTransactions = (await GetAllTransactionsAsync()).ToList();
                foreach (var transaction in transactions.Where(transaction => existingTransactions.All(t => t.NxtId != transaction.NxtId)))
                {
                    context.Transactions.Add(transaction);
                }
                await context.SaveChangesAsync();
            }
        }

        public static async Task SaveBalanceAsync(string balance)
        {
            using (var context = new WalletContext())
            {
                var dbBalance = await context.Settings.SingleOrDefaultAsync(s => s.Key.Equals(BalanceKey));
                if (dbBalance == null)
                {
                    context.Settings.Add(new Setting {Key = BalanceKey, Value = balance});
                }
                else
                {
                    dbBalance.Value = balance;
                }
                await context.SaveChangesAsync();
                Balance = balance;
            }
        }

        public static async Task LoadAsync()
        {
            using (var context = new WalletContext())
            {
                CreateAndMigrateDb(context);

                var dbSettings = await context.Settings.ToListAsync();

                ReadOrGenerateSecretPhrase(dbSettings, context);
                ReadOrGenerateNxtServer(dbSettings, context);
                ReadOrGenerateBalance(dbSettings, context);

                NxtAccount = new LocalAccountService().GetAccount(AccountIdLocator.BySecretPhrase(SecretPhrase));
                await context.SaveChangesAsync();
            }
        }

        private static void ReadOrGenerateBalance(IEnumerable<Setting> dbSettings, WalletContext context)
        {
            Balance = dbSettings.SingleOrDefault(s => s.Key.Equals(BalanceKey))?.Value;
            if (Balance == null)
            {
                Balance = "0.0";
                context.Settings.Add(new Setting {Key = BalanceKey, Value = Balance});
            }
        }

        private static void ReadOrGenerateNxtServer(IEnumerable<Setting> dbSettings, WalletContext context)
        {
            NxtServer = dbSettings.SingleOrDefault(s => s.Key.Equals(NxtServerKey))?.Value;
            if (NxtServer == null)
            {
                NxtServer = Constants.DefaultNxtUrl;
                context.Settings.Add(new Setting {Key = NxtServerKey, Value = NxtServer});
            }
        }

        private static void ReadOrGenerateSecretPhrase(IEnumerable<Setting> dbSettings, WalletContext context)
        {
            SecretPhrase = dbSettings.SingleOrDefault(s => s.Key.Equals(SecretPhraseKey))?.Value;
            if (SecretPhrase == null)
            {
                var generator = new LocalPasswordGenerator();
                SecretPhrase = generator.GeneratePassword();
                context.Settings.Add(new Setting {Key = SecretPhraseKey, Value = SecretPhrase});
            }
        }

        private static void CreateAndMigrateDb(WalletContext context)
        {
            context.Database.Migrate();
        }
    }
}
