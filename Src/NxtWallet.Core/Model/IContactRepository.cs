using System.Collections.Generic;
using System.Threading.Tasks;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public interface IContactRepository
    {
        Task<IList<Contact>> GetAllContactsAsync();
        Task UpdateContactAsync(Contact contact);
        Task<Contact> AddContactAsync(Contact contact);
        Task DeleteContactAsync(Contact contact);
        Task<IList<Contact>> GetContactsAsync(IEnumerable<string> nxtRsAddresses);
    }
}