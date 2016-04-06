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
        private readonly IContactRepository _contactRepository;
        private ObservableCollection<Contact> _contacts;
        private Contact _selectedContact;

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { Set(ref _contacts, value); }
        }

        public Contact SelectedContact
        {
            get { return _selectedContact; }
            set { Set(ref _selectedContact, value); }
        }

        public ICommand SaveSelectedContact { get; }

        public ContactsViewModel(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
            SaveSelectedContact = new RelayCommand(SaveContact);
            LoadContacts();
        }

        private async void LoadContacts()
        {
            var contacts = await _contactRepository.GetAllContacts();
            Contacts = new ObservableCollection<Contact>(contacts.OrderBy(c => c.Name));
        }

        private async void SaveContact()
        {
            //await _walletRepository.UpdateContact(SelectedContact);
        }
    }
}