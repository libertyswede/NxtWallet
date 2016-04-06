using System;
using GalaSoft.MvvmLight;

namespace NxtWallet.ViewModel.Model
{
    public class TransactionModel : ObservableObject, IEquatable<TransactionModel>
    {
        private bool _isConfirmed;

        public int Id { get; set; }
        public ulong NxtId { get; set; }
        public DateTime Timestamp { get; set; }
        public long NqtAmount { get; set; }
        public string FormattedAmount => (UserIsRecipient ? "" : "-") + FormattedAmountAbsolute;
        public string FormattedAmountAbsolute => (NqtAmount / (decimal)100000000).ToFormattedString();
        public long NqtFee { get; set; }
        public string FormattedFee => UserIsRecipient ? string.Empty : "-" + FormattedFeeAbsolute;
        public string FormattedFeeAbsolute => (NqtFee / (decimal)100000000).ToFormattedString();
        public long NqtBalance { get; set; }
        public string FormattedBalance => (NqtBalance / (decimal)100000000).ToFormattedString();
        public string AccountFrom { get; set; }
        public string ContactListAccountFrom => UserIsRecipient ? AccountFrom : "you";
        public string AccountTo { get; set; }
        public string ContactListAccountTo => UserIsRecipient ? "you" : AccountTo;
        public string Message { get; set; }
        public bool UserIsRecipient { get; set; }

        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set { Set(ref _isConfirmed, value); }
        }

        public override bool Equals(object obj)
        {
            var transaction = obj as TransactionModel;
            return transaction != null && Equals(transaction);
        }

        public override int GetHashCode()
        {
            return NxtId.GetHashCode();
        }

        public bool Equals(TransactionModel other)
        {
            return other?.NxtId == NxtId;
        }

        public bool IsReceived(string yourAccountRs)
        {
            return AccountTo == yourAccountRs;
        }
    }
}
