using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NxtWallet.Core.Model;
using NxtWallet.Core.ViewModel.Model;

namespace NxtWallet.ViewModel
{
    public class ContactsViewModel : ViewModelBase
    {
        private readonly IContactRepository _contactRepository;
        private readonly INavigationService _navigationService;
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
                SendMoneyCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveSelectedContact { get; }
        public RelayCommand SendMoneyCommand { get; }

        public ContactsViewModel(IContactRepository contactRepository, INavigationService navigationService)
        {
            _contactRepository = contactRepository;
            _navigationService = navigationService;
            SaveSelectedContact = new RelayCommand(SaveContact, () => SelectedContact != null);
            AddCommand = new RelayCommand(AddContact);
            DeleteCommand = new RelayCommand(DeleteContact, () => SelectedContact != null);
            SendMoneyCommand = new RelayCommand(SendMoney, () => SelectedContact != null);
        }

        private void SendMoney()
        {
            _navigationService.NavigateTo(NavigationPage.SendMoneyPage, SelectedContact);
        }

        public async void LoadFromRepository()
        {
            var contacts = (await _contactRepository.GetAllContactsAsync()).OrderBy(c => c.Name).ToList();
            Contacts = new ObservableCollection<Contact>(contacts);
        }

        private async void SaveContact()
        {
            await _contactRepository.UpdateContactAsync(SelectedContact);
        }

        private async void AddContact()
        {
            var newContact = new Contact
            {
                Name = "name",
                NxtAddressRs = "NXT-"
            };
            newContact = await _contactRepository.AddContactAsync(newContact);
            Contacts.Add(newContact);
            SelectedContact = newContact;
        }

        private async void DeleteContact()
        {
            if (SelectedContact == null)
                return;

            Contacts.Remove(SelectedContact);
            await _contactRepository.DeleteContactAsync(SelectedContact);
            SelectedContact = null;
        }
    }
}