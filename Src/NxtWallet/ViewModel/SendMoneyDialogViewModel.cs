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
            private set { Set(ref _isDone, value); }
        }

        public string Message
        {
            get { return _message; }
            private set { Set(ref _message, value); }
        }

        public void Init()
        {
            Message = "Please wait...";
            IsDone = false;
        }

        public void SetDone()
        {
            Message = "Success";
            IsDone = true;
        }

        public void SetError(string message)
        {
            Message = message;
            IsDone = true;
        }
    }
}
