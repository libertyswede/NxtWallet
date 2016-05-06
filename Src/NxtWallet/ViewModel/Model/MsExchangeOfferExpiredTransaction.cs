using System;

namespace NxtWallet.ViewModel.Model
{
    public class MsExchangeOfferExpiredTransaction : Transaction, IEquatable<MsExchangeOfferExpiredTransaction>, IEquatable<Transaction>
    {
        public long CurrencyId { get; set; }
        public long OfferId { get; set; }

        public override bool Equals(object obj)
        {
            var transaction = obj as MsExchangeOfferExpiredTransaction;
            return transaction != null && Equals(transaction);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ OfferId.GetHashCode();
            }
        }

        public override bool Equals(Transaction other)
        {
            var transaction = other as MsExchangeOfferExpiredTransaction;
            return transaction != null && Equals(transaction);
        }

        public bool Equals(MsExchangeOfferExpiredTransaction other)
        {
            return other?.OfferId == OfferId;
        }
    }
}
