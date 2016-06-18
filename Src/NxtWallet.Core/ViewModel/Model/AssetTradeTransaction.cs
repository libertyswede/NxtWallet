namespace NxtWallet.ViewModel.Model
{
    public class AssetTradeTransaction : Transaction
    {
        public ulong AssetNxtId { get; set; }
        public long QuantityQnt { get; set; }
    }
}