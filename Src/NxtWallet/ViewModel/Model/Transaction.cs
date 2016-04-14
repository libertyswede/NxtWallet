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
        private bool _userIsSender;

        public int Id { get; set; }
        public ulong NxtId { get; set; }
        public DateTime Timestamp { get; set; }
        public long NqtAmount { get; set; }
        public string FormattedAmount => (UserIsRecipient || NqtAmount == 0 ? "" : "-") + FormattedAmountAbsolute;
        public string FormattedAmountAbsolute => (NqtAmount / (decimal)100000000).ToFormattedString();
        public long NqtFee { get; set; }
        public string FormattedFee => UserIsSender ? "-" + FormattedFeeAbsolute : string.Empty;
        public string FormattedFeeAbsolute => (NqtFee / (decimal)100000000).ToFormattedString();
        public long NqtBalance { get; set; }
        public string FormattedBalance => (NqtBalance / (decimal)100000000).ToFormattedString();
        public string AccountFrom { get; set; }
        public string ContactListAccountFrom { get; private set; }
        public string AccountTo { get; set; }
        public string ContactListAccountTo { get; private set; }
        public string Message { get; set; }
        public TransactionType TransactionType { get; set; }
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
        ForgeIncome = 1002
    }
}
