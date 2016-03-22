using System;
using NxtWallet.Model;

namespace NxtWallet.ViewModel
{
    public class ViewModelTransaction : IEquatable<ViewModelTransaction>
    {
        public long NxtId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Amount { get; set; }
        public string Fee { get; set; }
        public string Balance { get; set; }
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }
        public string OtherAccount { get; set; }
        public string Message { get; set; }
        public bool UserIsRecipient { get; set; }
        public ITransaction Transaction { get; set; }

        public ViewModelTransaction(ITransaction transaction, string myAccountRs)
        {
            UserIsRecipient = myAccountRs.Equals(transaction.AccountTo);
            NxtId = transaction.NxtId;
            Timestamp = transaction.Timestamp;
            Amount = (transaction.NqtAmount/(decimal) 100000000).ToFormattedString();
            Balance = (transaction.NqtBalance/(decimal) 100000000).ToFormattedString();
            Fee = (transaction.NqtFeeAmount/(decimal) 100000000).ToFormattedString();
            Message = transaction.Message;
            AccountFrom = UserIsRecipient ? transaction.AccountFrom : "you";
            AccountTo = UserIsRecipient ? "you" : transaction.AccountTo;
            OtherAccount = UserIsRecipient ? transaction.AccountFrom : transaction.AccountTo;
            Transaction = transaction;

            if (UserIsRecipient)
            {
                Fee = string.Empty;
            }
            else
            {
                Amount = "-" + Amount;
                Fee = "-" + Fee;
            }
        }

        public override bool Equals(object obj)
        {
            var transaction = obj as ViewModelTransaction;
            return transaction != null && Equals(transaction);
        }

        public override int GetHashCode()
        {
            return NxtId.GetHashCode();
        }

        public bool Equals(ViewModelTransaction other)
        {
            return other?.NxtId == NxtId;
        }
    }
}
