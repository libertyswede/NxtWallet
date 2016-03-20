using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : ViewModelBase
    {
        private readonly INxtServer _nxtServer;
        private readonly IWalletRepository _walletRepository;
        
        private string _recipient;
        private string _amount;
        private string _message;

        public string Recipient
        {
            get { return _recipient; }
            set { Set(ref _recipient, value); }
        }

        public string Amount
        {
            get { return _amount; }
            set { Set(ref _amount, value); }
        }

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        public ICommand SendMoneyCommand { get; }

        public SendMoneyViewModel(INxtServer nxtServer, IWalletRepository walletRepository)
        {
            _nxtServer = nxtServer;
            _walletRepository = walletRepository;
            SendMoneyCommand = new RelayCommand(SendMoney, CanSendMoney);
        }

        private async void SendMoney()
        {
            await Task.Run(async () =>
            {
                decimal amount;
                decimal.TryParse(Amount, out amount);
                var transaction = await _nxtServer.SendMoneyAsync(Recipient, NxtLib.Amount.CreateAmountFromNxt(amount), Message);
                await _walletRepository.SaveTransactionAsync(transaction);
            });
        }

        private bool CanSendMoney()
        {
            return _nxtServer.OnlineStatus == OnlineStatus.Online;
        }
    }
}
