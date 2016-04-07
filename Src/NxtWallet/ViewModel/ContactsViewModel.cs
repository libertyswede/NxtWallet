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

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveSelectedContact { get; }

        public ContactsViewModel(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
            SaveSelectedContact = new RelayCommand(SaveContact);
            AddCommand = new RelayCommand(AddContact);
            DeleteCommand = new RelayCommand(DeleteContact);
        }

        public async void LoadFromRepository()
        {
            var contacts = (await _contactRepository.GetAllContacts()).OrderBy(c => c.Name).ToList();
            Contacts = new ObservableCollection<Contact>(contacts);
        }

        private async void SaveContact()
        {
            //await _walletRepository.UpdateContact(SelectedContact);
        }

        private async void AddContact()
        {
            var newContact = new Contact
            {
                Name = "name",
                NxtAddressRs = "NXT-"
            };
            newContact = await _contactRepository.AddContact(newContact);
            Contacts.Add(newContact);
            SelectedContact = newContact;
        }

        private async void DeleteContact()
        {
            if (SelectedContact == null)
                return;
            Contacts.Remove(SelectedContact);
            await _contactRepository.DeleteContact(SelectedContact);
            SelectedContact = null;
        }
    }
}