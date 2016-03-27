using System;

namespace NxtWallet.Model
{
    public interface ITransaction
    {
        int Id { get; set; }
        long NxtId { get; set; }
        DateTime Timestamp { get; set; }
        long NqtAmount { get; set; }
        long NqtFeeAmount { get; set; }
        long NqtBalance { get; set; }
        string AccountFrom { get; set; }
        string AccountTo { get; set; }
        string Message { get; set; }
        bool IsConfirmed { get; set; }

        bool IsReceived(string yourAddressRs);
        ulong GetTransactionId();
        bool Equals(object obj);
        int GetHashCode();
        bool Equals(ITransaction other);
    }

    public class Transaction : IEquatable<ITransaction>, ITransaction
    {
        public int Id { get; set; }
        public long NxtId { get; set; }
        public DateTime Timestamp { get; set; }
        public long NqtAmount { get; set; }
        public long NqtFeeAmount { get; set; }
        public long NqtBalance { get; set; }
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }
        public string Message { get; set; }
        public bool IsConfirmed { get; set; }

        public Transaction()
        {
        }

        public Transaction(NxtLib.Transaction nxtTransaction, long? transactionId = null)
        {
            if (transactionId.HasValue)
            {
                NxtId = transactionId.Value;
            }
            else if (nxtTransaction.TransactionId.HasValue)
            {
                NxtId = (long)nxtTransaction.TransactionId.Value;
            }
            else
            {
                throw new ArgumentException("Transaction id must be set either explicitly or on Transaction object.", nameof(transactionId));
            }
            Message = nxtTransaction.Message?.MessageText;
            Timestamp = nxtTransaction.Timestamp;
            NqtAmount = nxtTransaction.Amount.Nqt;
            NqtBalance = 0;
            NqtFeeAmount = nxtTransaction.Fee.Nqt;
            AccountFrom = nxtTransaction.SenderRs;
            AccountTo = nxtTransaction.RecipientRs;
            IsConfirmed = nxtTransaction.Confirmations.HasValue && nxtTransaction.Confirmations.Value > 0;
        }

        public bool IsReceived(string yourAddressRs)
        {
            return AccountTo.Equals(yourAddressRs);
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

        public bool Equals(ITransaction other)
        {
            return other?.NxtId == NxtId;
        }
    }
}