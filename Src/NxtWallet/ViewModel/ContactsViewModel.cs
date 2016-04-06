using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NxtWallet.Model;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.ViewModel
{
    public class ContactsViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private ObservableCollection<ContactModel> _contacts;
        private ContactModel _selectedContact;

        public ObservableCollection<ContactModel> Contacts
        {
            get { return _contacts; }
            set { Set(ref _contacts, value); }
        }

        public ContactModel SelectedContact
        {
            get { return _selectedContact; }
            set { Set(ref _selectedContact, value); }
        }

        public ICommand SaveSelectedContact { get; }

        public ContactsViewModel(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
            SaveSelectedContact = new RelayCommand(SaveContact);
            LoadContacts();
        }

        private async void LoadContacts()
        {
            var contacts = await _walletRepository.GetAllContacts();
            Contacts = new ObservableCollection<ContactModel>(contacts.OrderBy(c => c.Name));
        }

        private async void SaveContact()
        {
            //await _walletRepository.UpdateContact(SelectedContact);
        }
    }
}