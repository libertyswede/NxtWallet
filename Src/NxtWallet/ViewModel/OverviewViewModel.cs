using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtWallet.Model;

namespace NxtWallet.ViewModel
{
    public class OverviewViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly INxtServer _nxtServer;

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

        public OverviewViewModel(IWalletRepository walletRepository, INxtServer nxtServer,
            IBackgroundRunner backgroundRunner)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;

            Balance = "0.0";
            NxtAddress = walletRepository.NxtAccount.AccountRs;

            backgroundRunner.BalanceUpdated += (sender, balance) => Balance = balance;
        }

        public void LoadFromRepository()
        {
            Balance = _walletRepository.Balance;
            ShowAddress = _walletRepository.BackupCompleted;
        }

        public async Task LoadFromNxtServerAsync()
        {
            var balanceResult = await _nxtServer.GetBalanceAsync();
            if (balanceResult.Success)
            {
                Balance = balanceResult.Value;
                await _walletRepository.SaveBalanceAsync(Balance);
            }
        }
    }
}