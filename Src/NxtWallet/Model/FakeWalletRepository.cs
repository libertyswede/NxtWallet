using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxtLib;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public class FakeWalletRepository : IWalletRepository
    {
        public AccountWithPublicKey NxtAccount { get; set; } = new AccountWithPublicKey("NXT-5XAB-J4KK-5JKF-EA42X", "f91588343ba5a14e2a4960b2bfcf027e44e0d9337f683e0169d0e021714d3313");
        public string NxtServer { get; set; }
        public string SecretPhrase { get; set; }
        public string Balance { get; set; } = "1100000000";
        public bool BackupCompleted { get; } = false;

        public Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        public Task SaveBalanceAsync(string balance)
        {
            return Task.CompletedTask;
        }

        public Task UpdateNxtServer(string newServerAddress)
        {
            return Task.CompletedTask;
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

        public Task UpdateContact(ContactModel contactModel)
        {
            return Task.CompletedTask;
        }
    }
}