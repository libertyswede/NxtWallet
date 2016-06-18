using System;

namespace NxtWallet.ViewModel.Model
{
    public class ShufflingDistributionTransaction : Transaction, IEquatable<Transaction>, IEquatable<ShufflingDistributionTransaction>
    {
        public long ShufflingId { get; set; }
        public string RecipientPublicKey { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)TransactionType * 397) ^ ShufflingId.GetHashCode() * RecipientPublicKey.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            var transaction = obj as ShufflingDistributionTransaction;
            return transaction != null && Equals(transaction);
        }

        public override bool Equals(Transaction other)
        {
            var transaction = other as ShufflingDistributionTransaction;
            return transaction != null && Equals(transaction);
        }

        public bool Equals(ShufflingDistributionTransaction other)
        {
            return other?.ShufflingId == ShufflingId && other?.RecipientPublicKey == RecipientPublicKey;
        }
    }
}
