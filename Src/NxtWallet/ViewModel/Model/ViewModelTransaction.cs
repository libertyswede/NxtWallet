using System;
using GalaSoft.MvvmLight;
using NxtWallet.Model;

namespace NxtWallet.ViewModel.Model
{
    public class ViewModelTransaction : ObservableObject, IEquatable<ViewModelTransaction>
    {
        private bool _isConfirmed;

        public int Id { get; }
        public ulong NxtId { get; }
        public DateTime Timestamp { get; }
        public long NqtAmount { get; }
        public string FormattedAmount { get; }
        public string FormattedAmountAbsolute { get; private set; }
        public long NqtFee { get; }
        public string FormattedFee { get; }
        public string FormattedFeeAbsolute { get; private set; }
        public long NqtBalance { get; private set; }
        public string FormattedBalance { get; private set; }
        public string AccountFrom { get; }
        public string ContactListAccountFrom { get; }
        public string AccountTo { get; }
        public string ContactListAccountTo { get; }
        public string Message { get; }
        public bool UserIsRecipient { get; }

        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set { Set(ref _isConfirmed, value); }
        }

        public ViewModelTransaction(ITransaction transaction, string myAccountRs)
        {
            UserIsRecipient = myAccountRs.Equals(transaction.AccountTo);
            Id = transaction.Id;
            NxtId = (ulong)transaction.NxtId;
            Timestamp = transaction.Timestamp;
            NqtAmount = transaction.NqtAmount;
            NqtBalance = transaction.NqtBalance;
            NqtFee = transaction.NqtFeeAmount;
            FormattedAmountAbsolute = FormattedAmount = (NqtAmount/(decimal) 100000000).ToFormattedString();
            FormattedBalance = (NqtBalance/(decimal) 100000000).ToFormattedString();
            FormattedFeeAbsolute = FormattedFee = (NqtFee/(decimal) 100000000).ToFormattedString();
            Message = transaction.Message;
            AccountFrom = transaction.AccountFrom;
            ContactListAccountFrom = UserIsRecipient ? transaction.AccountFrom : "you";
            AccountTo = transaction.AccountTo;
            ContactListAccountTo = UserIsRecipient ? "you" : transaction.AccountTo;
            IsConfirmed = transaction.IsConfirmed;

            if (UserIsRecipient)
            {
                FormattedFee = string.Empty;
            }
            else
            {
                FormattedAmount = "-" + FormattedAmount;
                FormattedFee = "-" + FormattedFee;
            }
        }

        public void SetBalance(long balance)
        {
            NqtBalance = balance;
            FormattedBalance = (balance / (decimal)100000000).ToFormattedString();
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

        public bool IsReceived(string yourAccountRs)
        {
            return AccountTo == yourAccountRs;
        }
    }
}
