namespace NxtWallet.ViewModel
{
    public class SendMoneyDialogMessage
    {
        public enum DialogState
        {
            Progress,
            Done,
            Error
        }

        public DialogState State { get; set; }
        public string ErrorMessage { get; set; }
    }
}