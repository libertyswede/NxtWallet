using System.Collections.Generic;
using System.Threading.Tasks;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public interface IContactRepository
    {
        Task<IList<Contact>> GetAllContacts();
        Task UpdateContact(Contact contact);
        Task<Contact> AddContact(Contact contact);
        Task DeleteContact(Contact contact);
        Task<IList<Contact>> GetContacts(IEnumerable<string> nxtRsAddresses);
    }
}