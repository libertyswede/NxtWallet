using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using NxtLib;

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
        public DateTime Timestamp { get; set; }
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
        public string Message { get; set; }
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

        public bool UserIsAmountRecipient => UserIsAmountRecipientCalculation();

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

        public long GetAmount()
        {
            return NqtAmount;
        }

        public long GetBalance()
        {
            return NqtBalance;
        }

        public long GetFee()
        {
            return NqtFee;
        }

        public long GetOrder()
        {
            return Timestamp.Ticks;
        }

        public void SetBalance(long balance)
        {
            NqtBalance = balance;
        }

        private bool UserIsAmountRecipientCalculation()
        {
            if (LedgerEntryType == LedgerEntryType.AssetDividendPayment)
            {
                return !UserIsSender;
            }
            if (LedgerEntryType == LedgerEntryType.CurrencyUndoCrowdfunding ||
                LedgerEntryType == LedgerEntryType.CurrencyReserveClaim ||
                LedgerEntryType == LedgerEntryType.CurrencyExchangeSell)
            {
                return true;
            }
            return UserIsRecipient != (LedgerEntryType == LedgerEntryType.DigitalGoodsDelivery);
        }
    }
}
