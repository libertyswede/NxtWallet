using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtWallet.Core;
using GalaSoft.MvvmLight.Threading;
using NxtWallet.Core.Repositories;
using NxtWallet.Core.Models;
using System;

namespace NxtWallet.ViewModel
{
    public class LedgerEntryListViewModel : ViewModelBase
    {
        private readonly IAccountLedgerRepository _accountLedgerRepository;
        private readonly IContactRepository _contactRepository;
        private ObservableCollection<LedgerEntry> _ledgerEntries;

        public ObservableCollection<LedgerEntry> LedgerEntries
        {
            get { return _ledgerEntries; }
            set { Set(ref _ledgerEntries, value); }
        }

        public LedgerEntryListViewModel(IAccountLedgerRepository accountLedgerRepository, IAccountLedgerRunner accountLedgerRunner,
            IContactRepository contactRepository)
        {
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
            MessengerInstance.Register<SecretPhraseResetMessage>(this, (message) => LedgerEntries.Clear());

            _accountLedgerRepository = accountLedgerRepository;
            _contactRepository = contactRepository;
            LedgerEntries = new ObservableCollection<LedgerEntry>();
        }

        public void LoadLedgerEntriesFromRepository()
        {
            var ledgerEntries = Task.Run(async () => await _accountLedgerRepository.GetAllLedgerEntriesAsync()).Result.ToList();
            var contacts = Task.Run(async () => await _contactRepository.GetAllContactsAsync())
                .Result
                .ToDictionary(contact => contact.NxtAddressRs);
            ledgerEntries.ForEach(t => t.UpdateWithContactInfo(contacts));
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
            return LedgerEntries.FirstOrDefault(t => t.Timestamp.CompareTo(ledgerEntry.Timestamp) < 0);
        }

        private int? GetPreviousLedgerEntryIndex(LedgerEntry ledgerEntry)
        {
            var previousLedgerEntry = GetPreviousLedgerEntry(ledgerEntry);
            return previousLedgerEntry == null ? null : (int?)LedgerEntries.IndexOf(previousLedgerEntry);
        }
    }
}
