using System.Collections.ObjectModel;
using System.Linq;
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
            set
            {
                if (_selectedContact == value)
                    return;

                Set(ref _selectedContact, value);
                DeleteCommand.RaiseCanExecuteChanged();
                SaveSelectedContact.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveSelectedContact { get; }

        public ContactsViewModel(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
            SaveSelectedContact = new RelayCommand(SaveContact, () => SelectedContact != null);
            AddCommand = new RelayCommand(AddContact);
            DeleteCommand = new RelayCommand(DeleteContact, () => SelectedContact != null);
        }

        public async void LoadFromRepository()
        {
            var contacts = (await _contactRepository.GetAllContacts()).OrderBy(c => c.Name).ToList();
            Contacts = new ObservableCollection<Contact>(contacts);
        }

        private async void SaveContact()
        {
            await _contactRepository.UpdateContact(SelectedContact);
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