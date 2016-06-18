using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NxtWallet.Migrations.Model
{
    [Table("Setting")]
    public class TransactionDto : IEquatable<TransactionDto>
    {
        public int Id { get; set; }
        public long? NxtId { get; set; }
        public DateTime Timestamp { get; set; }
        public long NqtAmount { get; set; }
        public long NqtFee { get; set; }
        public long NqtBalance { get; set; }
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }
        public string Message { get; set; }
        public bool IsConfirmed { get; set; }
        public int TransactionType { get; set; }
        public int Height { get; set; }
        public string Extra { get; set; }

        public override bool Equals(object obj)
        {
            var transaction = obj as TransactionDto;
            return transaction != null && Equals(transaction);
        }

        public override int GetHashCode()
        {
            return NxtId.GetHashCode() ^ TransactionType.GetHashCode() ^ Extra.GetHashCode();
        }

        public bool Equals(TransactionDto other)
        {
            return other?.NxtId == NxtId && other?.TransactionType == TransactionType && other?.Extra == Extra;
        }
    }
}