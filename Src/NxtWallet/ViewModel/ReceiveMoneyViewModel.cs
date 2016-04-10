using System;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using NxtWallet.Model;
using NxtWallet.Views;
using ZXing;

namespace NxtWallet.ViewModel
{
    public class ReceiveMoneyViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IBackupInfoDialog _backupInfoDialog;
        private readonly ITransactionRepository _transactionRepository;

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

        public ReceiveMoneyViewModel(IWalletRepository walletRepository, IBackupInfoDialog backupInfoDialog,
            ITransactionRepository transactionRepository)
        {
            _walletRepository = walletRepository;
            _backupInfoDialog = backupInfoDialog;
            _transactionRepository = transactionRepository;
            NxtAddress = walletRepository.NxtAccount.AccountRs;
            PublicKey = walletRepository.NxtAccount.PublicKey.ToHexString();

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

        public async void OnNavigatedTo()
        {
            ShowNxtAddress = _walletRepository.BackupCompleted;
            ShowPublicKey = ShowNxtAddress && !(await _transactionRepository.HasOutgoingTransactionAsync());

            if (!ShowNxtAddress)
            {
                await _backupInfoDialog.ShowAsync();
            }
        }
    }
}
