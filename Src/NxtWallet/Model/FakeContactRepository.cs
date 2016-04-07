using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public class FakeContactRepository : IContactRepository
    {
        public Task<IEnumerable<Contact>> GetAllContacts()
        {
            var contacts = new List<Contact>
            {
                new Contact {Name = "MrV777", NxtAddressRs = "NXT-BK2J-ZMY4-93UY-8EM9V"},
                new Contact {Name = "bitcoinpaul", NxtAddressRs = "NXT-M5JR-2L5Z-CFBP-8X7P3"},
                new Contact {Name = "EvilDave", NxtAddressRs = "NXT-BNZB-9V8M-XRPW-3S3WD"},
                new Contact {Name = "coretechs", NxtAddressRs = "NXT-WY9K-ZMTT-QQTT-3NBL7"},
                new Contact {Name = "Damelon", NxtAddressRs = "NXT-D6K7-MLY6-98FM-FLL5T"}
            };
            return Task.FromResult(contacts.AsEnumerable());
        }

        public Task UpdateContact(Contact contact)
        {
            return Task.CompletedTask;
        }

        public Task<Contact> AddContact(Contact contact)
        {
            return Task.FromResult(contact);
        }

        public Task DeleteContact(Contact contact)
        {
            return Task.CompletedTask;
        }
    }
}