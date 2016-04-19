using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using NxtLib;

namespace NxtWallet.ViewModel.Model
{
    public class Transaction : ObservableObject, IEquatable<Transaction>, ILedgerEntry
    {
        private bool _isConfirmed;
        private bool _userIsTransactionRecipient;
        private bool _userIsTransactionSender;

        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public ulong? NxtId { get; set; }

        [JsonIgnore]
        public DateTime Timestamp { get; set; }

        [JsonIgnore]
        public long NqtAmount { get; set; }

        [JsonIgnore]
        public string FormattedAmount => (UserIsAmountRecipient || NqtAmount == 0 ? "" : "-") + FormattedAmountAbsolute;

        [JsonIgnore]
        public string FormattedAmountAbsolute => (NqtAmount / (decimal)100000000).ToFormattedString();

        [JsonIgnore]
        public long NqtFee { get; set; }

        [JsonIgnore]
        public string FormattedFee => UserIsTransactionSender ? "-" + FormattedFeeAbsolute : string.Empty;

        [JsonIgnore]
        public string FormattedFeeAbsolute => (NqtFee / (decimal)100000000).ToFormattedString();

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
        public TransactionType TransactionType { get; set; }

        [JsonIgnore]
        public int Height { get; set; }

        [JsonIgnore]
        public Attachment Attachment { get; set; }

        [JsonIgnore]
        public string Extra { get; set; }

        [JsonIgnore]
        public bool UserIsTransactionRecipient
        {
            get { return _userIsTransactionRecipient; }
            set
            {
                _userIsTransactionRecipient = value;
                ContactListAccountTo = _userIsTransactionRecipient ? "you" : AccountTo;
            }
        }

        [JsonIgnore]
        public bool UserIsTransactionSender
        {
            get { return _userIsTransactionSender; }
            set
            {
                _userIsTransactionSender = value;
                ContactListAccountFrom = _userIsTransactionSender ? "you" : AccountFrom;
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
            if (TransactionType == TransactionType.DividendPayment)
            {
                return !UserIsTransactionSender;
            }
            return UserIsTransactionRecipient != (TransactionType == TransactionType.DigitalGoodsDelivery);
        }
    }

    public enum TransactionType
    {
        // Payment
        OrdinaryPayment,

        // Messaging
        ArbitraryMessage,
        AliasAssignment,
        PollCreation,
        VoteCasting,
        HubTerminalAnnouncement,
        AccountInfo,
        AliasSell,
        AliasBuy,
        AliasDelete,
        PhasingVoteCasting,
        AccountProperty,
        AccountPropertyDelete,

        // ColoredCoins
        AssetIssuance,
        AssetTransfer,
        AskOrderPlacement,
        BidOrderPlacement,
        AskOrderCancellation,
        BidOrderCancellation,
        DividendPayment,
        AssetDelete,

        // DigitalGoods
        DigitalGoodsListing,
        DigitalGoodsDelisting,
        DigitalGoodsPriceChange,
        DigitalGoodsQuantityChange,
        DigitalGoodsPurchase,
        DigitalGoodsDelivery,
        DigitalGoodsFeedback,
        DigitalGoodsRefund,

        // AccountControl
        EffectiveBalanceLeasing,
        SetPhasingOnly,

        // MonetarySystem
        CurrencyIssuance,
        ReserveIncrease,
        ReserveClaim,
        CurrencyTransfer,
        PublishExchangeOffer,
        ExchangeBuy,
        ExchangeSell,
        CurrencyMinting,
        CurrencyDeletion,

        // TaggedData
        TaggedDataUpload,
        TaggedDataExtend,

        // Shuffling
        ShufflingCreation,
        ShufflingRegistration,
        ShufflingProcessing,
        ShufflingRecipients,
        ShufflingVerification,
        ShufflingCancellation,

        // Technically not transactions
        AssetTrade = 1001,
        ForgeIncome = 1002,
        DigitalGoodsPurchaseExpired = 1003
    }
}
