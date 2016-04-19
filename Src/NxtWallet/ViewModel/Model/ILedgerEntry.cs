namespace NxtWallet.ViewModel.Model
{
    public interface ILedgerEntry
    {
        bool UserIsTransactionSender { get; }
        bool UserIsAmountRecipient { get; }
        long GetAmount();
        long GetBalance();
        long GetFee();
        long GetOrder();
        void SetBalance(long balance);
    }
}