namespace NxtWallet.Core
{
    public class BalanceUpdatedMessage
    {
        public long NqtBalance { get; }

        public BalanceUpdatedMessage(long nqtBalance)
        {
            NqtBalance = nqtBalance;
        }
    }
}
