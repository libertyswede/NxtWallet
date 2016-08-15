using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using NxtLib;
using System.Text.RegularExpressions;

namespace NxtWallet.Core.Models
{
    public class LedgerEntry : ObservableObject, IEquatable<LedgerEntry>
    {
        public const string GeneratedFromAddress = "[Generated]";

        private bool _isConfirmed;
        private bool _userIsRecipient;
        private bool _userIsSender;

        public int Id { get; set; }
        public ulong? TransactionId { get; set; }
        public int? Height { get; set; }
        public ulong? BlockId { get; set; }
        public DateTime BlockTimestamp { get; set; }
        public DateTime TransactionTimestamp { get; set; }
        public long NqtAmount { get; set; }
        public decimal NxtAmount => (NqtAmount / 100000000M);
        public long NqtFee { get; set; }
        public decimal NxtFee => (NqtFee / 100000000M);
        public long NqtBalance { get; set; }
        public decimal NxtBalance => (NqtBalance / 100000000M);
        public string AccountFrom { get; set; }
        public string ContactListAccountFrom { get; private set; }
        public string AccountTo { get; set; }
        public string ContactListAccountTo { get; private set; }
        public string PlainMessage { get; set; }
        public string EncryptedMessage { get; set; }
        public string NoteToSelfMessage { get; set; }
        public string OverviewMessage { get; set; }
        public LedgerEntryType LedgerEntryType { get; set; }
        public Transaction Transaction { get; set; }
        public Attachment Attachment { get; set; }

        public bool UserIsRecipient
        {
            get { return _userIsRecipient; }
            set
            {
                _userIsRecipient = value;
                ContactListAccountTo = _userIsRecipient ? "you" : AccountTo;
            }
        }

        public bool UserIsSender
        {
            get { return _userIsSender; }
            set
            {
                _userIsSender = value;
                ContactListAccountFrom = _userIsSender ? "you" : AccountFrom;
            }
        }

        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set { Set(ref _isConfirmed, value); }
        }

        public void UpdateWithContactInfo(IList<Contact> contacts)
        {
            if (!UserIsSender)
            {
                ContactListAccountFrom = contacts.SingleOrDefault(c => c.NxtAddressRs.Equals(AccountFrom))?.Name ?? AccountFrom;
            }
            if (!UserIsRecipient && AccountTo != null)
            {
                ContactListAccountTo = contacts.SingleOrDefault(c => c.NxtAddressRs.Equals(AccountTo))?.Name ?? AccountTo;
            }
        }

        public void UpdateWithContactInfo(IDictionary<string, Contact> contacts)
        {
            Contact contact;
            
            if (!UserIsSender)
            {
                ContactListAccountFrom = contacts.TryGetValue(AccountFrom, out contact) ? contact.Name : AccountFrom;
            }
            if (!UserIsRecipient && AccountTo != null)
            {
                ContactListAccountTo = contacts.TryGetValue(AccountTo, out contact) ? contact.Name : AccountTo;
            }
        }

        public override bool Equals(object obj)
        {
            var ledgerEntry = obj as LedgerEntry;
            return ledgerEntry != null && Equals(ledgerEntry);
        }

        public override int GetHashCode()
        {
            return TransactionId.GetHashCode();
        }

        public virtual bool Equals(LedgerEntry other)
        {
            return other?.TransactionId == TransactionId && other?.LedgerEntryType == LedgerEntryType;
        }

        public void UpdateOverviewMessage()
        {
            if (!string.IsNullOrEmpty(NoteToSelfMessage))
            {
                OverviewMessage = NoteToSelfMessage;
            }
            else if (!string.IsNullOrEmpty(EncryptedMessage))
            {
                OverviewMessage = EncryptedMessage;
            }
            else if (!string.IsNullOrEmpty(PlainMessage))
            {
                OverviewMessage = PlainMessage;
            }
            else
            {
                var input = "[" + LedgerEntryType + "]";
                OverviewMessage = Regex.Replace(input, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled).Trim();
            }
        }
    }
}
