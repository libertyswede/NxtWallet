using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NxtWallet.Model
{
    [Table("Transaction")]
    public class TransactionDto : IEquatable<TransactionDto>
    {
        public int Id { get; set; }
        public long NxtId { get; set; }
        public DateTime Timestamp { get; set; }
        public long NqtAmount { get; set; }
        public long NqtFee { get; set; }
        public long NqtBalance { get; set; }
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }
        public string Message { get; set; }
        public bool IsConfirmed { get; set; }

        public override bool Equals(object obj)
        {
            var transaction = obj as TransactionDto;
            return transaction != null && Equals(transaction);
        }

        public override int GetHashCode()
        {
            return NxtId.GetHashCode();
        }

        public bool Equals(TransactionDto other)
        {
            return other?.NxtId == NxtId;
        }
    }
}