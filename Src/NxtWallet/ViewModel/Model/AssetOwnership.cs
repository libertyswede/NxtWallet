 using System;

namespace NxtWallet.ViewModel.Model
{
    public class AssetOwnership : IEquatable<AssetOwnership>, ILedgerEntry
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int TransactionId { get; set; }
        public long QuantityQnt { get; set; }
        public long BalanceQnt { get; set; }
        public decimal Quantity => QuantityQnt/(decimal) Math.Pow(10, AssetDecimals);
        public int AssetDecimals { get; set; }
        public int Height { get; set; }
        public Transaction Transaction { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssetOwnership) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (AssetId*397) ^ (TransactionId != 0 ? TransactionId : (int)Transaction.NxtId);
            }
        }

        public bool Equals(AssetOwnership other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return AssetId == other.AssetId &&
                   ((other.TransactionId != 0 && other.TransactionId == TransactionId) ||
                    (other.Transaction.NxtId == Transaction.NxtId));
        }

        public long GetAmount()
        {
            return QuantityQnt;
        }

        public long GetBalance()
        {
            return BalanceQnt;
        }

        public long GetFee()
        {
            return 0;
        }

        public long GetOrder()
        {
            return Height;
        }

        public void SetBalance(long balance)
        {
            BalanceQnt = balance;
        }

        public bool UserIsSender()
        {
            return false;
        }
    }
}