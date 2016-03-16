namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : BindableBase
    {
        private string _recipient;
        private string _amount;
        private string _message;

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
    }
}
