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
            var transaction = obj as MsUndoCrowdfundingTransaction;
            return transaction != null && Equals(transaction);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ TransactionNxtId.GetHashCode();
            }
        }

        public override bool Equals(Transaction other)
        {
            var transaction = other as MsUndoCrowdfundingTransaction;
            return transaction != null && Equals(transaction);
        }

        public bool Equals(MsCurrencyExchangeTransaction other)
        {
            return other?.OfferNxtId == OfferNxtId && other.TransactionNxtId == TransactionNxtId;
        }
    }
}