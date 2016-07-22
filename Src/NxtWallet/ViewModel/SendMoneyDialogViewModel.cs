using GalaSoft.MvvmLight;

namespace NxtWallet.ViewModel
{
    public class SendMoneyDialogViewModel : ViewModelBase
    {
        private bool _isDone;
        private string _message;

        public bool IsDone
        {
            get { return _isDone; }
            set { Set(ref _isDone, value); }
        }

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        public SendMoneyDialogViewModel()
        {
            Message = "Please wait...";
            IsDone = false;

            MessengerInstance.Register<SendMoneyDialogMessage>(this, (message) => OnMessage(message));
        }

        private void OnMessage(SendMoneyDialogMessage message)
        {
            if (message.State == SendMoneyDialogMessage.DialogState.Progress)
            {
                Message = "Please wait...";
            }
            else if (message.State == SendMoneyDialogMessage.DialogState.Done)
            {
                Message = "Success";
            }
            else if (message.State == SendMoneyDialogMessage.DialogState.Error)
            {
                Message = message.ErrorMessage;
            }

            IsDone = message.State != SendMoneyDialogMessage.DialogState.Progress;
        }
    }
}
