using GalaSoft.MvvmLight;
using NxtWallet.Model;

namespace NxtWallet.ViewModel
{
    public class ReceiveMoneyViewModel : ViewModelBase
    {
        private string _nxtAddress;
        private string _publicKey;

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

        public ReceiveMoneyViewModel(IWalletRepository walletRepository)
        {
            NxtAddress = walletRepository.NxtAccount.AccountRs;
            PublicKey = walletRepository.NxtAccount.PublicKey.ToHexString();
        }
    }
}
