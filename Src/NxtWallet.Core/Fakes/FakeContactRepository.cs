using System.Collections.Generic;
using System.Threading.Tasks;
using NxtWallet.Core.Models;
using NxtWallet.Repositories.Model;

namespace NxtWallet.Core.Fakes
{
    public class FakeContactRepository : IContactRepository
    {
        public List<Contact> GetAllContacts { get; set; }
        public Dictionary<string, Contact> GetContacts { get; set; }
        public List<Contact> UpdateContact { get; set; }
        public List<Contact> AddContact { get; set; }
        public List<Contact> DeleteContact { get; set; }

        public FakeContactRepository()
        {
            GetAllContacts = new List<Contact>();
            GetContacts = new Dictionary<string, Contact>();
            UpdateContact = new List<Contact>();
            AddContact = new List<Contact>();
            DeleteContact = new List<Contact>();

            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                UseDesignTimeData();
            }
        }

        public Task<List<Contact>> GetAllContactsAsync()
        {
            return Task.FromResult(GetAllContacts);
        }

        public Task<List<Contact>> GetContactsAsync(IEnumerable<string> nxtRsAddresses)
        {
            List<Contact> contacts = new List<Contact>();
            foreach (var address in nxtRsAddresses)
            {
                Contact contact = null;
                if (GetContacts.TryGetValue(address, out contact))
                {
                    contacts.Add(contact);
                }
            }
            return Task.FromResult(contacts);
        }

        public Task UpdateContactAsync(Contact contact)
        {
            UpdateContact.Add(contact);
            return Task.CompletedTask;
        }

        public Task<Contact> AddContactAsync(Contact contact)
        {
            AddContact.Add(contact);
            return Task.FromResult(contact);
        }

        public Task DeleteContactAsync(Contact contact)
        {
            DeleteContact.Add(contact);
            return Task.CompletedTask;
        }

        private void UseDesignTimeData()
        {
            GetAllContacts = new List<Contact>
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