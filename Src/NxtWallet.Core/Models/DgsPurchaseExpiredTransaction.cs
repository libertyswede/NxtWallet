using System;

namespace NxtWallet.Core.Models
{
    public class DgsPurchaseExpiredTransaction : Transaction, IEquatable<DgsPurchaseExpiredTransaction>
    {
        public long PurchaseTransactionNxtId { get; set; }

        public bool Equals(DgsPurchaseExpiredTransaction other)
        {
            return other?.PurchaseTransactionNxtId == PurchaseTransactionNxtId;
        }
    }
}