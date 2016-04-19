using System;

namespace NxtWallet.ViewModel.Model
{
    public class DgsPurchaseTransaction : Transaction
    {
        public long? DeliveryTransactionNxtId { get; set; }
        public DateTime DeliveryDeadlineTimestamp { get; set; }
    }
}