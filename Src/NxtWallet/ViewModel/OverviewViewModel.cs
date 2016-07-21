using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using NxtWallet.Core;
using NxtWallet.Core.Repositories;

namespace NxtWallet.ViewModel
{
    public class OverviewViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;

        private decimal _nxtBalance;
        private string _nxtAddress;
        private bool _showAddress;

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

        public OverviewViewModel(IWalletRepository walletRepository, IAccountLedgerRunner accountLedgerRunner)
        {
            _walletRepository = walletRepository;

            NxtBalance = 0M;
            NxtAddress = walletRepository.NxtAccount.AccountRs;

            accountLedgerRunner.BalanceUpdated += (sender, balance) => DispatcherHelper.CheckBeginInvokeOnUI(() => NxtBalance = balance.NqtToNxt());
        }

        public void LoadFromRepository()
        {
            NxtBalance = _walletRepository.NqtBalance.NqtToNxt();
            ShowAddress = _walletRepository.BackupCompleted;
        }
    }
}