using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : ViewModelBase
    {
        private readonly NxtServer _nxtServer;
        private string _recipient;
        private string _amount;
        private string _message;
        private ICommand _sendMoneyCommand;

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

        public ICommand SendMoneyCommand => _sendMoneyCommand ?? (_sendMoneyCommand = new CommandHandler(SendMoney, true));

        public SendMoneyViewModel(IWalletRepository walletRepository)
        {
            _nxtServer = new NxtServer(walletRepository);
        }

        private async void SendMoney()
        {
            await Task.Run(async () =>
            {
                decimal amount;
                decimal.TryParse(Amount, out amount);
                await _nxtServer.SendMoney(Recipient, NxtLib.Amount.CreateAmountFromNxt(amount), Message);
            });
        }
    }
}
