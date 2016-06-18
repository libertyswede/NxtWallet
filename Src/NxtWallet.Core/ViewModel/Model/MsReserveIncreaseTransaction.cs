namespace NxtWallet.ViewModel.Model
{
    public class MsReserveIncreaseTransaction : Transaction
    {
        public int IssuanceHeight { get; set; }
        public long CurrencyId { get; set; }
    }
}