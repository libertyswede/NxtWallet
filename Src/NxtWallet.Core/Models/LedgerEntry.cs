using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using NxtLib;

namespace NxtWallet.Core.Models
{
    public class LedgerEntry : ObservableObject, IEquatable<LedgerEntry>
    {
        public const string GeneratedFromAddress = "[Generated]";

        private bool _isConfirmed;
        private bool _userIsRecipient;
        private bool _userIsSender;

        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public ulong? TransactionId { get; set; }

        [JsonIgnore]
        public DateTime Timestamp { get; set; }

        [JsonIgnore]
        public long NqtAmount { get; set; }

        [JsonIgnore]
        public string FormattedAmount => (NqtAmount / (decimal)100000000).ToFormattedString();

        [JsonIgnore]
        public long NqtFee { get; set; }

        [JsonIgnore]
        public string FormattedFee => NqtFee < 0 ? (NqtFee / (decimal)100000000).ToFormattedString() : string.Empty;

        [JsonIgnore]
        public long NqtBalance { get; set; }

        [JsonIgnore]
        public string FormattedBalance => (NqtBalance / (decimal)100000000).ToFormattedString();

        [JsonIgnore]
        public string AccountFrom { get; set; }

        [JsonIgnore]
        public string ContactListAccountFrom { get; private set; }

        [JsonIgnore]
        public string AccountTo { get; set; }

        [JsonIgnore]
        public string ContactListAccountTo { get; private set; }

        [JsonIgnore]
        public string Message { get; set; }

        [JsonIgnore]
        public LedgerEntryType LedgerEntryType { get; set; }

        [JsonIgnore]
        public int Height { get; set; }

        [JsonIgnore]
        public Transaction Transaction { get; set; }

        [JsonIgnore]
        public Attachment Attachment { get; set; }

        [JsonIgnore]
        public bool UserIsTransactionRecipient
        {
            get { return _userIsRecipient; }
            set
            {
                _userIsRecipient = value;
                ContactListAccountTo = _userIsRecipient ? "you" : AccountTo;
            }
        }

        [JsonIgnore]
        public bool UserIsTransactionSender
        {
            get { return _userIsSender; }
            set
            {
                _userIsSender = value;
                ContactListAccountFrom = _userIsSender ? "you" : AccountFrom;
            }
        }

        [JsonIgnore]
        public bool UserIsAmountRecipient => UserIsAmountRecipientCalculation();

        [JsonIgnore]
        public bool IsConfirmed
        {
            get { return _isConfirmed; }
            set { Set(ref _isConfirmed, value); }
        }

        public void UpdateWithContactInfo(IList<Contact> contacts)
        {
            if (!UserIsTransactionSender)
            {
                ContactListAccountFrom = contacts.SingleOrDefault(c => c.NxtAddressRs.Equals(AccountFrom))?.Name ?? AccountFrom;
            }
            if (!UserIsTransactionRecipient && AccountTo != null)
            {
                ContactListAccountTo = contacts.SingleOrDefault(c => c.NxtAddressRs.Equals(AccountTo))?.Name ?? AccountTo;
            }
        }

        public void UpdateWithContactInfo(IDictionary<string, Contact> contacts)
        {
            Contact contact;
            
            if (!UserIsTransactionSender)
            {
                ContactListAccountFrom = contacts.TryGetValue(AccountFrom, out contact) ? contact.Name : AccountFrom;
            }
            if (!UserIsTransactionRecipient && AccountTo != null)
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
                return !UserIsTransactionSender;
            }
            if (LedgerEntryType == LedgerEntryType.CurrencyUndoCrowdfunding ||
                LedgerEntryType == LedgerEntryType.CurrencyReserveClaim ||
                LedgerEntryType == LedgerEntryType.CurrencyExchangeSell)
            {
                return true;
            }
            return UserIsTransactionRecipient != (LedgerEntryType == LedgerEntryType.DigitalGoodsDelivery);
        }
    }
}
