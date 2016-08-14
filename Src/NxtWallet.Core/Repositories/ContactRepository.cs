using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NxtWallet.Core.Models;
using NxtWallet.Core.Migrations.Model;
using NxtWallet.Repositories.Model;
using System;
using Microsoft.EntityFrameworkCore;

namespace NxtWallet.Core.Repositories
{
    public interface IContactRepository
    {
        Task<List<Contact>> GetAllContactsAsync();
        Task UpdateContactAsync(Contact contact);
        Task<Contact> AddContactAsync(Contact contact);
        Task DeleteContactAsync(Contact contact);
        Task<List<Contact>> GetContactsAsync(IEnumerable<string> nxtRsAddresses);
        Task<Contact> GetContactAsync(string rsAddress);
        Task<List<Contact>> SearchContactsContainingNameOrAddressText(string text);
    }

    public class ContactRepository : IContactRepository
    {
        private readonly IMapper _mapper;

        public ContactRepository(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<List<Contact>> GetAllContactsAsync()
        {
            using (var context = new WalletContext())
            {
                var contactsDto = await context.Contacts.ToListAsync();
                return _mapper.Map<List<Contact>>(contactsDto);
            }
        }

        public async Task UpdateContactAsync(Contact contact)
        {
            await UpdateEntityStateAsync(contact, EntityState.Modified);
        }

        public async Task<Contact> AddContactAsync(Contact contact)
        {
            using (var context = new WalletContext())
            {
                var contactDto = _mapper.Map<ContactDto>(contact);
                context.Contacts.Add(contactDto);
                await context.SaveChangesAsync();
                return _mapper.Map<Contact>(contactDto);
            }
        }

        public async Task DeleteContactAsync(Contact contact)
        {
            await UpdateEntityStateAsync(contact, EntityState.Deleted);
        }

        public async Task<List<Contact>> GetContactsAsync(IEnumerable<string> nxtRsAddresses)
        {
            using (var context = new WalletContext())
            {
                var list = await context.Contacts.Where(c => nxtRsAddresses.Contains(c.NxtAddressRs)).ToListAsync();
                return _mapper.Map<List<Contact>>(list);
            }
        }

        public async Task<Contact> GetContactAsync(string rsAddress)
        {
            using (var context = new WalletContext())
            {
                var contact = await context.Contacts.SingleOrDefaultAsync(c => c.NxtAddressRs == rsAddress);
                return _mapper.Map<Contact>(contact);
            }
        }

        public async Task<List<Contact>> SearchContactsContainingNameOrAddressText(string text)
        {
            using (var context = new WalletContext())
            {
                var contacts = await context.Contacts.Where(c => c.Name.Contains(text) || c.NxtAddressRs.Contains(text))
                    .Distinct()
                    .ToListAsync();
                return _mapper.Map<List<Contact>>(contacts).OrderBy(c => c.Name).ToList();
            }
        }

        private async Task UpdateEntityStateAsync(Contact contact, EntityState entityState)
        {
            using (var context = new WalletContext())
            {
                var contactDto = _mapper.Map<ContactDto>(contact);
                context.Contacts.Attach(contactDto);
                context.Entry(contactDto).State = entityState;
                await context.SaveChangesAsync();
            }
        }
    }
}