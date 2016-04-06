using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using NxtLib;
using NxtLib.Accounts;
using NxtLib.Local;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public class WalletRepository : IWalletRepository
    {
        private const string SecretPhraseKey = "secretPhrase";
        private const string BackupCompletedKey = "backupCompleted";
        private const string NxtServerKey = "nxtServer";
        private const string BalanceKey = "balance";

        public AccountWithPublicKey NxtAccount { get; private set; }
        public string NxtServer { get; private set; }
        public string SecretPhrase { get; private set; }
        public bool BackupCompleted { get; private set; }
        public string Balance { get; private set; }

        public async Task LoadAsync()
        {
            using (var context = new WalletContext())
            {
                CreateAndMigrateDb(context);

                var dbSettings = await context.Settings.ToListAsync();

                ReadOrGenerateSecretPhrase(dbSettings, context);
                ReadOrGenerateNxtServer(dbSettings, context);
                ReadOrGenerateBalance(dbSettings, context);
                ReadOrGenerateBackupCompleted(dbSettings, context);

                NxtAccount = new LocalAccountService().GetAccount(AccountIdLocator.BySecretPhrase(SecretPhrase));
                await context.SaveChangesAsync();
            }
        }

        public async Task SaveBalanceAsync(string balance)
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

        public async Task UpdateNxtServer(string newServerAddress)
        {
            using (var context = new WalletContext())
            {
                var setting = context.Settings.Single(s => s.Key == NxtServerKey);
                setting.Value = newServerAddress;
                await context.SaveChangesAsync();
            }
        }

        public Task<IEnumerable<ContactModel>> GetAllContacts()
        {
            var contacts = new List<ContactModel>
            {
                new ContactModel {Name = "MrV777", NxtAddressRs = "NXT-BK2J-ZMY4-93UY-8EM9V"},
                new ContactModel {Name = "bitcoinpaul", NxtAddressRs = "NXT-M5JR-2L5Z-CFBP-8X7P3"},
                new ContactModel {Name = "EvilDave", NxtAddressRs = "NXT-BNZB-9V8M-XRPW-3S3WD"},
                new ContactModel {Name = "coretechs", NxtAddressRs = "NXT-WY9K-ZMTT-QQTT-3NBL7"},
                new ContactModel {Name = "Damelon", NxtAddressRs = "NXT-D6K7-MLY6-98FM-FLL5T"}
            };
            return Task.FromResult(contacts.AsEnumerable());
        }

        public async Task UpdateContact(ContactModel contactModel)
        {
            using (var context = new WalletContext())
            {
                var contact = new Contact
                {
                    Id = contactModel.Id,
                    Name = contactModel.Name,
                    NxtAddressRs = contactModel.NxtAddressRs
                };
                context.Contacts.Attach(contact);
                context.Entry(contact).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }

        private void ReadOrGenerateBalance(IEnumerable<Setting> dbSettings, WalletContext context)
        {
            Balance = dbSettings.SingleOrDefault(s => s.Key.Equals(BalanceKey))?.Value;
            if (Balance == null)
            {
                Balance = "0.0";
                context.Settings.Add(new Setting {Key = BalanceKey, Value = Balance});
            }
        }

        private void ReadOrGenerateBackupCompleted(IEnumerable<Setting> dbSettings, WalletContext context)
        {
            var backupCompleted = dbSettings.SingleOrDefault(s => s.Key.Equals(BackupCompletedKey))?.Value;
            if (backupCompleted == null)
            {
                BackupCompleted = false;
                context.Settings.Add(new Setting {Key = BackupCompletedKey, Value = BackupCompleted.ToString()});
            }
            else
            {
                BackupCompleted = bool.Parse(backupCompleted);
            }
        }

        private void ReadOrGenerateNxtServer(IEnumerable<Setting> dbSettings, WalletContext context)
        {
            NxtServer = dbSettings.SingleOrDefault(s => s.Key.Equals(NxtServerKey))?.Value;
            if (NxtServer == null)
            {
                //NxtServer = Constants.DefaultNxtUrl;
                NxtServer = Constants.TestnetNxtUrl;
                context.Settings.Add(new Setting {Key = NxtServerKey, Value = NxtServer});
            }
        }

        private void ReadOrGenerateSecretPhrase(IEnumerable<Setting> dbSettings, WalletContext context)
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
