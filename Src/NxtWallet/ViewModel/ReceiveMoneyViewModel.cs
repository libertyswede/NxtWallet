using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using ZXing;
using NxtWallet.Core.Repositories;

namespace NxtWallet.ViewModel
{
    public class ReceiveMoneyViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IAccountLedgerRepository _accountLedgerRepository;
        private readonly INavigationService _navigationService;

        private string _nxtAddress;
        private string _publicKey;
        private bool _showNxtAddress;
        private WriteableBitmap _nxtAddressQr;
        private bool _showPublicKey;

        public string NxtAddress
        {
            get { return _nxtAddress; }
            set { Set(ref _nxtAddress, value); }
        }

        public bool ShowNxtAddress
        {
            get { return _showNxtAddress; }
            set { Set(ref _showNxtAddress, value); }
        }

        public string PublicKey
        {
            get { return _publicKey; }
            set { Set(ref _publicKey, value); }
        }

        public bool ShowPublicKey
        {
            get { return _showPublicKey; }
            set { Set(ref _showPublicKey, value); }
        }

        public WriteableBitmap NxtAddressQr
        {
            get { return _nxtAddressQr; }
            set { Set(ref _nxtAddressQr, value); }
        }

        public ReceiveMoneyViewModel(IWalletRepository walletRepository, IAccountLedgerRepository accountLedgerRepository,
            INavigationService navigationService)
        {
            _walletRepository = walletRepository;
            _accountLedgerRepository = accountLedgerRepository;
            _navigationService = navigationService;

            NxtAddress = walletRepository.NxtAccount.AccountRs;
            PublicKey = walletRepository.NxtAccountWithPublicKey?.PublicKey.ToHexString();

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options =
                {
                    Height = 200,
                    Width = 200,
                    Margin = 0
                }
            };
            NxtAddressQr = (WriteableBitmap) writer.Write(NxtAddress).ToBitmap();
        }

        public void OnNavigatedTo()
        {
            ShowNxtAddress = _walletRepository.BackupCompleted;
            ShowPublicKey = ShowNxtAddress;// && !(await _accountLedgerRepository.HasOutgoingTransactionAsync());

            if (!ShowNxtAddress)
            {
                _navigationService.ShowDialog(NavigationDialog.BackupInfo);
            }
        }
    }
}
