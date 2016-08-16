using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using NxtWallet.Core;
using NxtWallet.Core.Models;
using NxtWallet.Core.Repositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NxtWallet.ViewModel
{
    public class OverviewViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IAccountLedgerRepository _accountLedgerRepository;
        private readonly IContactRepository _contactRepository;

        private decimal _nxtBalance;
        private string _nxtAddress;
        private bool _showAddress;
        private ObservableCollection<LedgerEntry> _ledgerEntries;
        private LedgerEntry _selectedLedgerEntry;
        private string _selectedLedgerEntryLink;

        public string NxtAddress
        {
            get { return _nxtAddress; }
            set { Set(ref _nxtAddress, value); }
        }

        public decimal NxtBalance
        {
            get { return _nxtBalance; }
            set { Set(ref _nxtBalance, value); }
        }

        public bool ShowAddress
        {
            get { return _showAddress; }
            set { Set(ref _showAddress, value); }
        }

        public ObservableCollection<LedgerEntry> LedgerEntries
        {
            get { return _ledgerEntries; }
            set { Set(ref _ledgerEntries, value); }
        }

        public LedgerEntry SelectedLedgerEntry
        {
            get { return _selectedLedgerEntry; }
            set
            {
                if (_selectedLedgerEntry != value)
                {
                    Set(ref _selectedLedgerEntry, value);
                    UpdateTransactionLink();
                }
            }
        }

        public string SelectedLedgerEntryLink
        {
            get { return _selectedLedgerEntryLink; }
            set { Set(ref _selectedLedgerEntryLink, value); }
        }

        private void UpdateTransactionLink()
        {
            SelectedLedgerEntryLink = $"https://nxtportal.org/transactions/{SelectedLedgerEntry?.TransactionId}";
        }

        public OverviewViewModel(IWalletRepository walletRepository, IAccountLedgerRunner accountLedgerRunner, IAccountLedgerRepository accountLedgerRepository, 
            IContactRepository contactRepository)
        {
            _walletRepository = walletRepository;
            _accountLedgerRepository = accountLedgerRepository;
            _contactRepository = contactRepository;
            InitUiProperties();

            MessengerInstance.Register<LedgerEntryMessage>(this, (message) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    if (message.Action == LedgerEntryMessageAction.Added)
                    {
                        InsertLedgerEntry(message.LedgerEntry);
                    }
                    else if (message.Action == LedgerEntryMessageAction.ConfirmationUpdated)
                    {
                        var existingLedgerEntry = LedgerEntries.Single(t => t.Equals(message.LedgerEntry));
                        existingLedgerEntry.IsConfirmed = message.LedgerEntry.IsConfirmed;
                    }
                    else if (message.Action == LedgerEntryMessageAction.Removed)
                    {
                        LedgerEntries.Remove(LedgerEntries.Single(t => t.Equals(message.LedgerEntry)));
                    }
                });
            });

            MessengerInstance.Register<BalanceUpdatedMessage>(this, (message) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => NxtBalance = message.NqtBalance.NqtToNxt());
            });

            MessengerInstance.Register<SecretPhraseResetMessage>(this, (message) => 
            {
                InitUiProperties();
                LedgerEntries.Clear();
            });

            LedgerEntries = new ObservableCollection<LedgerEntry>();
            LoadLedgerEntriesFromRepository();
        }

        private void InitUiProperties()
        {
            NxtBalance = 0M;
            NxtAddress = _walletRepository.NxtAccount.AccountRs;
            UpdateTransactionLink();
        }

        public void LoadFromRepository()
        {
            NxtBalance = _walletRepository.NqtBalance.NqtToNxt();
            ShowAddress = _walletRepository.BackupCompleted;
        }

        private void LoadLedgerEntriesFromRepository()
        {
            var ledgerEntries = Task.Run(async () => await _accountLedgerRepository.GetAllLedgerEntriesAsync()).Result.ToList();
            var contacts = Task.Run(async () => await _contactRepository.GetAllContactsAsync())
                .Result
                .ToDictionary(contact => contact.NxtAddressRs);
            ledgerEntries.ForEach(e => e.UpdateWithContactInfo(contacts));
            InsertLedgerEntries(ledgerEntries);
        }
        private void InsertLedgerEntries(IEnumerable<LedgerEntry> ledgerEntries)
        {
            foreach (var ledgerEntry in ledgerEntries.Except(LedgerEntries))
            {
                InsertLedgerEntry(ledgerEntry);
            }
        }

        private void InsertLedgerEntry(LedgerEntry ledgerEntry)
        {
            if (!LedgerEntries.Any())
            {
                LedgerEntries.Add(ledgerEntry);
            }
            else
            {
                var index = GetPreviousLedgerEntryIndex(ledgerEntry);
                if (index.HasValue)
                {
                    LedgerEntries.Insert(index.Value, ledgerEntry);
                }
                else
                {
                    LedgerEntries.Add(ledgerEntry);
                }
            }
        }

        private LedgerEntry GetPreviousLedgerEntry(LedgerEntry ledgerEntry)
        {
            return LedgerEntries.FirstOrDefault(t => t.TransactionTimestamp.CompareTo(ledgerEntry.TransactionTimestamp) < 0 &&
                                                     t.BlockTimestamp.CompareTo(ledgerEntry.BlockTimestamp) < 0);
        }

        private int? GetPreviousLedgerEntryIndex(LedgerEntry ledgerEntry)
        {
            var previousLedgerEntry = GetPreviousLedgerEntry(ledgerEntry);
            return previousLedgerEntry == null ? null : (int?)LedgerEntries.IndexOf(previousLedgerEntry);
        }
    }
}