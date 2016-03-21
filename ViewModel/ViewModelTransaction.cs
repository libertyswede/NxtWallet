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
        public Transaction Transaction { get; set; }

        public ViewModelTransaction(Model.Transaction transaction, string myAccountRs)
        {
            UserIsRecipient = myAccountRs.Equals(transaction.AccountTo);
            NxtId = transaction.NxtId;
            Timestamp = transaction.Timestamp;
            Amount = NxtLib.Amount.CreateAmountFromNqt(transaction.NqtAmount).ToFormattedString();
            Balance = NxtLib.Amount.CreateAmountFromNqt(transaction.NqtBalance).ToFormattedString();
            Message = transaction.Message;
            AccountFrom = UserIsRecipient ? transaction.AccountFrom : "you";
            AccountTo = UserIsRecipient ? "you" : transaction.AccountTo;
            OtherAccount = UserIsRecipient ? transaction.AccountFrom : transaction.AccountTo;
            Transaction = transaction;

            if (UserIsRecipient)
            {
                Amount = NxtLib.Amount.CreateAmountFromNqt(transaction.NqtAmount).ToFormattedString();
                Fee = string.Empty;
            }
            else
            {
                Amount = "-" + NxtLib.Amount.CreateAmountFromNqt(transaction.NqtAmount).ToFormattedString();
                Fee = "-" + NxtLib.Amount.CreateAmountFromNqt(transaction.NqtFeeAmount).ToFormattedString();
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
