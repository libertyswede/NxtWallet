using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using NxtWallet.Model;
using ZXing;

namespace NxtWallet.ViewModel
{
    public class ReceiveMoneyViewModel : ViewModelBase
    {
        private string _nxtAddress;
        private string _publicKey;
        private WriteableBitmap _nxtAddressQr;

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

        public ReceiveMoneyViewModel(IWalletRepository walletRepository)
        {
            NxtAddress = walletRepository.NxtAccount.AccountRs;
            PublicKey = walletRepository.NxtAccount.PublicKey.ToHexString();

            var writer = new BarcodeWriter {Format = BarcodeFormat.QR_CODE};
            NxtAddressQr = (WriteableBitmap) writer.Write(NxtAddress).ToBitmap();
        }
    }
}
