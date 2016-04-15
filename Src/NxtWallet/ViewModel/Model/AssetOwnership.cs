 using System;

namespace NxtWallet.ViewModel.Model
{
    public class AssetOwnership
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int TransactionId { get; set; }
        public long QuantityQnt { get; set; }
        public decimal Quantity => QuantityQnt/(decimal) Math.Pow(10, AssetDecimals);
        public int AssetDecimals { get; set; }
        public int Height { get; set; }
        public Transaction Transaction { get; set; }
    }
}