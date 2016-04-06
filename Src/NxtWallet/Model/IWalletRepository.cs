using System.Collections.Generic;
using System.Threading.Tasks;
using NxtLib;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public interface IWalletRepository
    {
        AccountWithPublicKey NxtAccount { get; }
        string NxtServer { get; }
        string SecretPhrase { get; }
        string Balance { get; }
        bool BackupCompleted { get; }

        Task LoadAsync();
        Task SaveBalanceAsync(string balance);
        Task UpdateNxtServer(string newServerAddress);

        Task<IEnumerable<ContactModel>> GetAllContacts();
        Task UpdateContact(ContactModel contactModel);
    }
}