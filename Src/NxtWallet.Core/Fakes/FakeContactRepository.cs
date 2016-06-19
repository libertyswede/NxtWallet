using System.Collections.Generic;
using System.Threading.Tasks;
using NxtWallet.Core.Models;
using NxtWallet.Repositories.Model;

namespace NxtWallet.Core.Fakes
{
    public class FakeContactRepository : IContactRepository
    {
        public Task<IList<Contact>> GetAllContactsAsync()
        {
            IList<Contact> contacts = new List<Contact>
            {
                new Contact {Name = "MrV777", NxtAddressRs = "NXT-BK2J-ZMY4-93UY-8EM9V"},
                new Contact {Name = "bitcoinpaul", NxtAddressRs = "NXT-M5JR-2L5Z-CFBP-8X7P3"},
                new Contact {Name = "EvilDave", NxtAddressRs = "NXT-BNZB-9V8M-XRPW-3S3WD"},
                new Contact {Name = "coretechs", NxtAddressRs = "NXT-WY9K-ZMTT-QQTT-3NBL7"},
                new Contact {Name = "Damelon", NxtAddressRs = "NXT-D6K7-MLY6-98FM-FLL5T"}
            };
            return Task.FromResult(contacts);
        }

        public Task UpdateContactAsync(Contact contact)
        {
            return Task.CompletedTask;
        }

        public Task<Contact> AddContactAsync(Contact contact)
        {
            return Task.FromResult(contact);
        }

        public Task DeleteContactAsync(Contact contact)
        {
            return Task.CompletedTask;
        }

        public Task<IList<Contact>> GetContactsAsync(IEnumerable<string> nxtRsAddresses)
        {
            IList<Contact> contacts = new List<Contact>();
            return Task.FromResult(contacts);
        }
    }
}