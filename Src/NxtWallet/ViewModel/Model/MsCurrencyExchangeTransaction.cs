using System;

namespace NxtWallet.ViewModel.Model
{
    public class MsCurrencyExchangeTransaction : Transaction, IEquatable<MsCurrencyExchangeTransaction>, IEquatable<Transaction>
    {
        public long Units { get; set; }
        public long TransactionNxtId { get; set; }
        public long OfferNxtId { get; set; }

        public override bool Equals(object obj)
        {
            var transaction = obj as MsCurrencyExchangeTransaction;
            return transaction != null && Equals(transaction);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return OfferNxtId.GetHashCode() ^ TransactionNxtId.GetHashCode();
            }
        }

        public override bool Equals(Transaction other)
        {
            var transaction = other as MsCurrencyExchangeTransaction;
            return transaction != null && Equals(transaction);
        }

        public bool Equals(MsCurrencyExchangeTransaction other)
        {
            return other?.OfferNxtId == OfferNxtId && other.TransactionNxtId == TransactionNxtId;
        }
    }
}