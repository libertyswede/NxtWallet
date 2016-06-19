using System;

namespace NxtWallet.Core.Models
{
    public class ShufflingRefundTransaction : Transaction, IEquatable<Transaction>, IEquatable<ShufflingRefundTransaction>
    {
        public long ShufflingId { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)TransactionType * 397) ^ ShufflingId.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            var transaction = obj as ShufflingRefundTransaction;
            return transaction != null && Equals(transaction);
        }

        public override bool Equals(Transaction other)
        {
            var transaction = other as ShufflingRefundTransaction;
            return transaction != null && Equals(transaction);
        }

        public bool Equals(ShufflingRefundTransaction other)
        {
            return other?.ShufflingId == ShufflingId;
        }
    }
}
