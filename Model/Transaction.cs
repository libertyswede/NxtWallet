using System;

namespace NxtWallet.Model
{
    public class Transaction : IEquatable<Transaction>
    {
        public int Id { get; set; }
        public long NxtId { get; set; }
        public DateTime Timestamp { get; set; }
        public long NqtAmount { get; set; }
        public long NqtFeeAmount { get; set; }
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }
        public string Message { get; set; }

        public ulong GetTransactionId()
        {
            return (ulong) NxtId;
        }

        public override bool Equals(object obj)
        {
            var transaction = obj as Transaction;
            return transaction != null && Equals(transaction);
        }

        public override int GetHashCode()
        {
            return NxtId.GetHashCode();
        }

        public bool Equals(Transaction other)
        {
            return other?.NxtId == NxtId;
        }
    }
}