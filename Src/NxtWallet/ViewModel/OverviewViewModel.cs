using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using NxtWallet.Core;
using NxtWallet.Repositories.Model;

namespace NxtWallet.ViewModel
{
    public class OverviewViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;

        private string _balance;
        private string _nxtAddress;
        private bool _showAddress;

        public string NxtAddress
        {
            get { return _nxtAddress; }
            set { Set(ref _nxtAddress, value); }
        }

        public string Balance
        {
            get { return _balance; }
            set { Set(ref _balance, value); }
        }

        public bool ShowAddress
        {
            get { return _showAddress; }
            set { Set(ref _showAddress, value); }
        }

        public OverviewViewModel(IWalletRepository walletRepository, IBackgroundRunner backgroundRunner)
        {
            _walletRepository = walletRepository;

            Balance = "0.0";
            NxtAddress = walletRepository.NxtAccount.AccountRs;

            backgroundRunner.BalanceUpdated += (sender, balance) => DispatcherHelper.CheckBeginInvokeOnUI(() => Balance = balance);
        }

        public void LoadFromRepository()
        {
            Balance = _walletRepository.Balance;
            ShowAddress = _walletRepository.BackupCompleted;
        }
    }
}