using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Entity;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public class ContactRepository : IContactRepository
    {
        private readonly IMapper _mapper;

        public ContactRepository(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<IList<Contact>> GetAllContactsAsync()
        {
            using (var context = new WalletContext())
            {
                var contactsDto = await context.Contacts.ToListAsync();
                return _mapper.Map<IList<Contact>>(contactsDto);
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

        public async Task<IList<Contact>> GetContactsAsync(IEnumerable<string> nxtRsAddresses)
        {
            using (var context = new WalletContext())
            {
                var list = await context.Contacts.Where(c => nxtRsAddresses.Contains(c.NxtAddressRs)).ToListAsync();
                return _mapper.Map<IList<Contact>>(list);
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