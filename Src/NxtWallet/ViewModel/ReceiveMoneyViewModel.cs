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

        private string _nxtAddress;
        private string _publicKey;
        private bool _showAddress;
        private WriteableBitmap _nxtAddressQr;

        public bool ShowAddress
        {
            get { return _showAddress; }
            set { Set(ref _showAddress, value); }
        }

        public string NxtAddress
        {
            get { return _nxtAddress; }
            set { Set(ref _nxtAddress, value); }
        }

        public string PublicKey
        {
            get { return _publicKey; }
            set { Set(ref _publicKey, value); }
        }

        public WriteableBitmap NxtAddressQr
        {
            get { return _nxtAddressQr; }
            set { Set(ref _nxtAddressQr, value); }
        }

        public ReceiveMoneyViewModel(IWalletRepository walletRepository, IBackupInfoDialog backupInfoDialog)
        {
            _walletRepository = walletRepository;
            _backupInfoDialog = backupInfoDialog;
            NxtAddress = walletRepository.NxtAccount.AccountRs;
            PublicKey = walletRepository.NxtAccount.PublicKey.ToHexString();

            var writer = new BarcodeWriter {Format = BarcodeFormat.QR_CODE};
            NxtAddressQr = (WriteableBitmap) writer.Write(NxtAddress).ToBitmap();
        }

        public async void OnNavigatedTo()
        {
            ShowAddress = _walletRepository.BackupCompleted;
            if (!ShowAddress)
            {
                await _backupInfoDialog.ShowAsync();
            }
        }
    }
}
