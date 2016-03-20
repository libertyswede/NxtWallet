using System;

namespace NxtWallet.ViewModel
{
    public class TransactionViewModel
    {
        public DateTime Timestamp { get; set; }
        public string Amount { get; set; }
        public string Fee { get; set; }
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }
        public string Message { get; set; }
        public bool UserIsRecipient { get; set; }

        public TransactionViewModel(Model.Transaction transaction, string myAccountRs)
        {
            UserIsRecipient = myAccountRs.Equals(transaction.AccountFrom);
            Timestamp = transaction.Timestamp;
            Amount = NxtLib.Amount.CreateAmountFromNqt(transaction.NqtAmount).ToFormattedString();
            Fee = NxtLib.Amount.CreateAmountFromNqt(transaction.NqtFeeAmount).ToFormattedString();
            Message = transaction.Message;
            AccountFrom = UserIsRecipient ? transaction.AccountFrom : "you";
            AccountTo = UserIsRecipient ? "you" : transaction.AccountTo;

            if (!UserIsRecipient)
            {
                Fee = "-" + Fee;
                Amount = "-" + Amount;
            }
        }
    }
}
