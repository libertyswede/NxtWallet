using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : ViewModelBase
    {
        private readonly INxtServer _nxtServer;
        private string _recipient;
        private string _amount;
        private string _message;
        private ICommand _sendMoneyCommand;

        public string Recipient
        {
            get { return _recipient; }
            set
            {
                _recipient = value;
                RaisePropertyChanged();
            }
        }

        public string Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                RaisePropertyChanged();
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                RaisePropertyChanged();
            }
        }

        public ICommand SendMoneyCommand => _sendMoneyCommand ?? (_sendMoneyCommand = new CommandHandler(SendMoney, true));

        public SendMoneyViewModel(INxtServer nxtServer)
        {
            _nxtServer = nxtServer;
        }

        private async void SendMoney()
        {
            await Task.Run(async () =>
            {
                decimal amount;
                decimal.TryParse(Amount, out amount);
                await _nxtServer.SendMoneyAsync(Recipient, NxtLib.Amount.CreateAmountFromNxt(amount), Message);
            });
        }
    }
}
