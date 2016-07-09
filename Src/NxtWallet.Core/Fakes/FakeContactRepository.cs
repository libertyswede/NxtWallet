using System.Collections.Generic;
using System.Threading.Tasks;
using NxtWallet.Core.Models;
using System.Linq;
using NxtWallet.Core.Repositories;

namespace NxtWallet.Core.Fakes
{
    public class FakeContactRepository : IContactRepository
    {
        public List<Contact> Contacts { get; set; }

        public FakeContactRepository()
        {
            Contacts = new List<Contact>();

            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                UseDesignTimeData();
            }
        }

        public Task<List<Contact>> GetAllContactsAsync()
        {
            return Task.FromResult(Contacts);
        }

        public Task UpdateContactAsync(Contact contact)
        {
            var storedContact = Contacts.Single(c => c.Id == contact.Id);
            Contacts.Remove(storedContact);
            Contacts.Add(contact);
            return Task.CompletedTask;
        }

        public Task<Contact> AddContactAsync(Contact contact)
        {
            if (contact.Id == 0)
                contact.Id = Contacts.Any() ? Contacts.Max(c => c.Id) + 1 : 1;
            Contacts.Add(contact);
            return Task.FromResult(contact);
        }

        public Task DeleteContactAsync(Contact contact)
        {
            var storedContact = Contacts.Single(c => c.Id == contact.Id);
            Contacts.Remove(storedContact);
            return Task.CompletedTask;
        }

        public Task<List<Contact>> GetContactsAsync(IEnumerable<string> nxtRsAddresses)
        {
            var contacts = Contacts.Where(c => nxtRsAddresses.Contains(c.NxtAddressRs)).ToList();
            return Task.FromResult(contacts);
        }

        private void UseDesignTimeData()
        {
            Contacts = new List<Contact>
            {
                new Contact {Name = "MrV777", NxtAddressRs = "NXT-BK2J-ZMY4-93UY-8EM9V"},
                new Contact {Name = "bitcoinpaul", NxtAddressRs = "NXT-M5JR-2L5Z-CFBP-8X7P3"},
                new Contact {Name = "EvilDave", NxtAddressRs = "NXT-BNZB-9V8M-XRPW-3S3WD"},
                new Contact {Name = "coretechs", NxtAddressRs = "NXT-WY9K-ZMTT-QQTT-3NBL7"},
                new Contact {Name = "Damelon", NxtAddressRs = "NXT-D6K7-MLY6-98FM-FLL5T"}
            };
        }
    }
}