namespace NxtWallet.Core.Models
{
    public class MsPublishExchangeOfferTransaction : Transaction
    {
        public long CurrencyId { get; set; }
        public int ExpirationHeight { get; set; }
        public long BuyRateNqt { get; set; }
        public long BuySupply { get; set; }
        public long BuyLimit { get; set; }
        public long SellRateNqt { get; set; }
        public long SellSupply { get; set; }
        public long SellLimit { get; set; }
        public bool IsExpired { get; set; } = false;
    }
}