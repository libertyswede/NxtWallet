using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;

namespace NxtWallet.ViewModel.Model
{
    public class Transaction : ObservableObject, IEquatable<Transaction>
    {
        private bool _isConfirmed;
        private bool _userIsRecipient;

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
        public string ContactListAccountFrom { get; private set; }
        public string AccountTo { get; set; }
        public string ContactListAccountTo { get; private set; }
        public string Message { get; set; }
        public bool UserIsRecipient
        {
            get { return _userIsRecipient; }
            set
            {
                _userIsRecipient = value;
                if (_userIsRecipient)
                {
                    ContactListAccountTo = "you";
                }
                else
                {
                    ContactListAccountFrom = "you";
                }
            }
        }
        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set { Set(ref _isConfirmed, value); }
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

        public bool IsReceived(string yourAccountRs)
        {
            return AccountTo == yourAccountRs;
        }

        public void UpdateWithContactInfo(IList<Contact> contacts)
        {
            if (UserIsRecipient)
            {
                var contact = contacts.SingleOrDefault(c => c.NxtAddressRs.Equals(AccountFrom));
                ContactListAccountFrom = contact?.NxtAddressRs ?? ContactListAccountFrom;
            }
            else
            {
                var contact = contacts.SingleOrDefault(c => c.NxtAddressRs.Equals(AccountTo));
                ContactListAccountTo = contact?.NxtAddressRs ?? ContactListAccountTo;
            }
        }

        public void UpdateWithContactInfo(IDictionary<string, Contact> contacts)
        {
            Contact contact;
            
            if (UserIsRecipient && contacts.TryGetValue(AccountFrom, out contact))
            {
                ContactListAccountFrom = contact.Name;
            }
            else if (!UserIsRecipient && contacts.TryGetValue(AccountTo, out contact))
            {
                ContactListAccountTo = contact.Name;
            }
        }
    }
}
