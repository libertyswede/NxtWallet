using System;

namespace NxtWallet.Core.Models
{
    public class DgsPurchaseTransaction : Transaction
    {
        public long? DeliveryTransactionNxtId { get; set; }
        public DateTime DeliveryDeadlineTimestamp { get; set; }
    }
}