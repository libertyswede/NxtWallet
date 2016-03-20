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

        public Transaction()
        {
        }

        public Transaction(NxtLib.Transaction nxtTransaction)
        {
            // ReSharper disable once PossibleInvalidOperationException
            NxtId = (long) nxtTransaction.TransactionId.Value;
            Message = nxtTransaction.Message?.MessageText;
            Timestamp = nxtTransaction.Timestamp;
            NqtAmount = nxtTransaction.Amount.Nqt;
            NqtFeeAmount = nxtTransaction.Fee.Nqt;
            AccountFrom = nxtTransaction.SenderRs;
            AccountTo = nxtTransaction.RecipientRs;
        }

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