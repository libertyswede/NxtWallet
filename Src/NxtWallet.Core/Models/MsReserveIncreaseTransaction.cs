namespace NxtWallet.Core.Models
{
    public class MsReserveIncreaseTransaction : Transaction
    {
        public int IssuanceHeight { get; set; }
        public long CurrencyId { get; set; }
    }
}