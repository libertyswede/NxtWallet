using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NxtWallet.Core.Migrations.Model
{
    [Table("LedgerEntry")]
    public class LedgerEntryDto : IEquatable<LedgerEntryDto>
    {
        public int Id { get; set; }
        public long? TransactionId { get; set; }
        public int? Height { get; set; }
        public long? BlockId { get; set; }
        public DateTime Timestamp { get; set; }
        public long NqtBalance { get; set; }
        public long NqtAmount { get; set; }
        public long NqtFee { get; set; }
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }
        public string Message { get; set; }
        public bool IsConfirmed { get; set; }
        public int TransactionType { get; set; }

        public override bool Equals(object obj)
        {
            var ledgerEntry = obj as LedgerEntryDto;
            return ledgerEntry != null && Equals(ledgerEntry);
        }

        public override int GetHashCode()
        {
            return TransactionId.GetHashCode() ^ TransactionType.GetHashCode();
        }

        public bool Equals(LedgerEntryDto other)
        {
            return other?.TransactionId == TransactionId && other?.TransactionType == TransactionType;
        }
    }
}
