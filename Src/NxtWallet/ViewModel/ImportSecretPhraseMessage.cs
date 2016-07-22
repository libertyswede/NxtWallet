namespace NxtWallet.ViewModel
{
    public class ImportSecretPhraseMessage
    {
        public enum State
        {
            ShowInfo,
            Import,
            Imported
        }

        public State MessageState { get; set; } = State.ShowInfo;

        public ImportSecretPhraseMessage(State messageState = State.ShowInfo)
        {
            MessageState = messageState;
        }
    }
}