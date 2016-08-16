using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using NxtWallet.Core;
using NxtWallet.Core.Models;
using NxtWallet.Core.Repositories;

namespace NxtWallet.ViewModel
{
    public class OverviewViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;

        private decimal _nxtBalance;
        private string _nxtAddress;
        private bool _showAddress;
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

        public OverviewViewModel(IWalletRepository walletRepository, IAccountLedgerRunner accountLedgerRunner)
        {
            _walletRepository = walletRepository;
            InitUiProperties();

            MessengerInstance.Register<BalanceUpdatedMessage>(this, (message) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => NxtBalance = message.NqtBalance.NqtToNxt());
            });
            MessengerInstance.Register<SecretPhraseResetMessage>(this, (message) => InitUiProperties());
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
    }
}