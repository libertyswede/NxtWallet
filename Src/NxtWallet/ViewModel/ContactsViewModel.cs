using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using NxtWallet.Model;

namespace NxtWallet.ViewModel
{
    public class ContactsViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private ObservableCollection<Contact> _contacts;

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { Set(ref _contacts, value); }
        }

        public ContactsViewModel(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
            LoadContacts();
        }

        private async void LoadContacts()
        {
            var contacts = await _walletRepository.GetAllContacts();
            Contacts = new ObservableCollection<Contact>(contacts.OrderBy(c => c.Name));
        }
    }
}