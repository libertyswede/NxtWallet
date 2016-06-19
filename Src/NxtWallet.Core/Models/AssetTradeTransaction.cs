namespace NxtWallet.Core.Models
{
    public class AssetTradeTransaction : Transaction
    {
        public ulong AssetNxtId { get; set; }
        public long QuantityQnt { get; set; }
    }
}