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

        public virtual bool Equals(Transaction other)
        {
            return other?.NxtId == NxtId && other?.TransactionType == TransactionType;
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
            if (TransactionType == TransactionType.CurrencyUndoCrowdfunding ||
                TransactionType == TransactionType.ReserveClaim ||
                TransactionType == TransactionType.ExchangeSell)
            {
                return true;
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
        DigitalGoodsPurchaseExpired = 1003,
        CurrencyUndoCrowdfunding = 1004,
        CurrencyExchange = 1005,
        CurrencyOfferExpired = 1006
    }

    // NRS Account Ledger Events

    //public enum LedgerEvent
    //{
    //    // Block and Transaction
    //    BLOCK_GENERATED(1, false), ------------------------------ Supported
    //    REJECT_PHASED_TRANSACTION(2, true),  -------------------- Not supported
    //    TRANSACTION_FEE(50, true), ------------------------------ Supported
    //    // TYPE_PAYMENT 
    //    ORDINARY_PAYMENT(3, true), ------------------------------ Supported
    //    // TYPE_MESSAGING
    //    ACCOUNT_INFO(4, true),     ------------------------------ Supported
    //    ALIAS_ASSIGNMENT(5, true), ------------------------------ Supported
    //    ALIAS_BUY(6, true),        ------------------------------ Supported
    //    ALIAS_DELETE(7, true),     ------------------------------ Supported
    //    ALIAS_SELL(8, true),       ------------------------------ Supported
    //    ARBITRARY_MESSAGE(9, true),  ---------------------------- Supported
    //    HUB_ANNOUNCEMENT(10, true),  ---------------------------- Not supported
    //    PHASING_VOTE_CASTING(11, true),  ------------------------ Supported
    //    POLL_CREATION(12, true),   ------------------------------ Supported
    //    VOTE_CASTING(13, true),    ------------------------------ Supported
    //    ACCOUNT_PROPERTY(56, true),  ---------------------------- Supported
    //    ACCOUNT_PROPERTY_DELETE(57, true),  --------------------- Supported
    //    // TYPE_COLORED_COINS
    //    ASSET_ASK_ORDER_CANCELLATION(14, true),  ---------------- Supported
    //    ASSET_ASK_ORDER_PLACEMENT(15, true),  ------------------- Supported
    //    ASSET_BID_ORDER_CANCELLATION(16, true),  ---------------- Supported
    //    ASSET_BID_ORDER_PLACEMENT(17, true),  ------------------- Supported
    //    ASSET_DIVIDEND_PAYMENT(18, true),  ---------------------- Supported
    //    ASSET_ISSUANCE(19, true),  ------------------------------ Supported
    //    ASSET_TRADE(20, true),     ------------------------------ Supported
    //    ASSET_TRANSFER(21, true),  ------------------------------ Supported
    //    ASSET_DELETE(49, true),    ------------------------------ Supported
    //    // TYPE_DIGITAL_GOODS
    //    DIGITAL_GOODS_DELISTED(22, true), ------------------------ Supported
    //    DIGITAL_GOODS_DELISTING(23, true), ----------------------- Supported
    //    DIGITAL_GOODS_DELIVERY(24, true), ------------------------ Supported
    //    DIGITAL_GOODS_FEEDBACK(25, true), ------------------------ Supported
    //    DIGITAL_GOODS_LISTING(26, true),  ------------------------ Supported
    //    DIGITAL_GOODS_PRICE_CHANGE(27, true),  ------------------- Supported
    //    DIGITAL_GOODS_PURCHASE(28, true),  ----------------------- Supported
    //    DIGITAL_GOODS_PURCHASE_EXPIRED(29, true),  --------------- Supported
    //    DIGITAL_GOODS_QUANTITY_CHANGE(30, true),  ---------------- Supported
    //    DIGITAL_GOODS_REFUND(31, true),  ------------------------- Supported
    //    // TYPE_ACCOUNT_CONTROL
    //    ACCOUNT_CONTROL_EFFECTIVE_BALANCE_LEASING(32, true),  ---- Supported
    //    ACCOUNT_CONTROL_PHASING_ONLY(55, true),  ----------------- Supported
    //    // TYPE_CURRENCY
    //    CURRENCY_DELETION(33, true),  ----------------------------  Supported
    //    CURRENCY_DISTRIBUTION(34, true),  ------------------------  N/A
    //    CURRENCY_EXCHANGE(35, true),  ----------------------------  Supported
    //    CURRENCY_EXCHANGE_BUY(36, true),  ------------------------  Supported
    //    CURRENCY_EXCHANGE_SELL(37, true),  -----------------------  Supported
    //    CURRENCY_ISSUANCE(38, true),  ----------------------------  Supported
    //    CURRENCY_MINTING(39, true),  -----------------------------  Supported
    //    CURRENCY_OFFER_EXPIRED(40, true),  -----------------------  Supported
    //    CURRENCY_OFFER_REPLACED(41, true),  ----------------------  Supported
    //    CURRENCY_PUBLISH_EXCHANGE_OFFER(42, true),  --------------  Supported
    //    CURRENCY_RESERVE_CLAIM(43, true),  -----------------------  Supported
    //    CURRENCY_RESERVE_INCREASE(44, true),  --------------------  Supported
    //    CURRENCY_TRANSFER(45, true),  ----------------------------  Supported
    //    CURRENCY_UNDO_CROWDFUNDING(46, true),  -------------------  Supported
    //    // TYPE_DATA
    //    TAGGED_DATA_UPLOAD(47, true), ----------------------------  Supported
    //    TAGGED_DATA_EXTEND(48, true), ----------------------------  Supported
    //    // TYPE_SHUFFLING
    //    SHUFFLING_REGISTRATION(51, true), ------------------------
    //    SHUFFLING_PROCESSING(52, true), --------------------------
    //    SHUFFLING_CANCELLATION(53, true), ------------------------
    //    SHUFFLING_DISTRIBUTION(54, true); ------------------------
}
