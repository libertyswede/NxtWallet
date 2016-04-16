namespace NxtWallet.ViewModel.Model
{
    public interface ILedgerEntry
    {
        long GetAmount();
        long GetBalance();
        long GetFee();
        long GetOrder();
        void SetBalance(long balance);
        bool UserIsSender();
    }
}