using System.Windows.Input;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : BindableBase
    {
        private string _recipient;
        private string _amount;
        private string _message;
        private ICommand _sendMoneyCommand;

        public string Recipient
        {
            get { return _recipient; }
            set { SetProperty(ref _recipient, value); }
        }

        public string Amount
        {
            get { return _amount; }
            set { SetProperty(ref _amount, value); }
        }

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public ICommand SendMoneyCommand => _sendMoneyCommand ?? (_sendMoneyCommand = new CommandHandler(SendMoney, true));

        private static void SendMoney()
        {
        }
    }
}
