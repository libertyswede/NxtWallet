using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : ViewModelBase
    {
        private readonly INxtServer _nxtServer;
        private string _recipient;
        private string _amount;
        private string _message;
        private RelayCommand _sendMoneyCommand;

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

        public RelayCommand SendMoneyCommand => _sendMoneyCommand ?? (_sendMoneyCommand = new RelayCommand(SendMoney, CanSendMoney));

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
                var transaction = await _nxtServer.SendMoneyAsync(Recipient, NxtLib.Amount.CreateAmountFromNxt(amount), Message);
            });
        }

        private bool CanSendMoney()
        {
            return _nxtServer.OnlineStatus == OnlineStatus.Online;
        }
    }
}
