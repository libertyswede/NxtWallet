using System.Text.RegularExpressions;
using AutoMapper;
using NxtLib;
using NxtWallet.Core.Models;
using NxtWallet.Core.Migrations.Model;
using NxtLib.Accounts;
using System;

namespace NxtWallet.Core
{
    public class MapperConfig
    {
        private static MapperConfiguration _configuration;

        public static MapperConfiguration Setup()
        {
            if (_configuration != null)
                return _configuration;

            _configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ContactDto, Contact>();
                cfg.CreateMap<Contact, ContactDto>();
                cfg.CreateMap<LedgerEntryDto, LedgerEntry>()
                    .ForMember(dest => dest.BlockId, opt => opt.MapFrom(src => (ulong?)src.BlockId))
                    .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => (ulong?)src.TransactionId))
                    .ForMember(dest => dest.LedgerEntryType, opt => opt.MapFrom(src => (LedgerEntryType)src.TransactionType));

                cfg.CreateMap<LedgerEntry, LedgerEntryDto>()
                    .ForMember(dest => dest.BlockId, opt => opt.MapFrom(src => (long?)src.BlockId))
                    .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => (long?)src.TransactionId))
                    .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => (int)src.LedgerEntryType));

                cfg.CreateMap<AccountLedgerEntry, LedgerEntry>()
                    .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.IsTransactionEvent ? (ulong?)src.EventId : null))
                    .ForMember(dest => dest.NqtAmount, opt => opt.MapFrom(src => src.Change))
                    .ForMember(dest => dest.NqtBalance, opt => opt.MapFrom(src => src.Balance))
                    .ForMember(dest => dest.NqtFee, opt => opt.MapFrom(src => src.Transaction != null ? src.Transaction.Fee.Nqt : 0))
                    .ForMember(dest => dest.AccountFrom, opt => opt.MapFrom(src => src.Transaction != null ? src.Transaction.SenderRs : string.Empty))
                    .ForMember(dest => dest.AccountTo, opt => opt.MapFrom(src => src.Transaction != null ? src.Transaction.RecipientRs : string.Empty))
                    .ForMember(dest => dest.IsConfirmed, opt => opt.UseValue(true))
                    .ForMember(dest => dest.LedgerEntryType, opt => opt.MapFrom(src => GetLedgerEntryType(src)))
                    .ForMember(dest => dest.Message, opt => opt.MapFrom(src => GetMessage(src)))
                    .ForMember(dest => dest.Attachment, opt => opt.MapFrom(src => src.Transaction != null ? src.Transaction.Attachment : null));

                cfg.CreateMap<Transaction, LedgerEntry>()
                    .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionId))
                    .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                    .ForMember(dest => dest.NqtAmount, opt => opt.MapFrom(src => src.Amount.Nqt))
                    .ForMember(dest => dest.NqtFee, opt => opt.MapFrom(src => src.Fee.Nqt))
                    .ForMember(dest => dest.AccountFrom, opt => opt.MapFrom(src => src.SenderRs))
                    .ForMember(dest => dest.AccountTo, opt => opt.MapFrom(src => src.RecipientRs))
                    .ForMember(dest => dest.Message, opt => opt.MapFrom(src => GetMessage(src)))
                    .ForMember(dest => dest.LedgerEntryType, opt => opt.MapFrom(src => (LedgerEntryType)(int)src.SubType))
                    .ForMember(dest => dest.IsConfirmed, opt => opt.MapFrom(src => src.Confirmations != null));

            });

            return _configuration;
        }

        private static string GetMessage(Transaction transaction)
        {
            if (transaction.SubType == TransactionSubType.PaymentOrdinaryPayment ||
                 transaction.SubType == TransactionSubType.MessagingArbitraryMessage)
            {
                return transaction.Message?.MessageText;
            }
            var input = "[" + (LedgerEntryType)(int)transaction.SubType + "]";
            return Regex.Replace(input, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }

        private static string GetMessage(AccountLedgerEntry accountLedgerEntry)
        {
            if (accountLedgerEntry.IsTransactionEvent && accountLedgerEntry.Transaction != null && 
                (accountLedgerEntry.Transaction.SubType == TransactionSubType.PaymentOrdinaryPayment ||
                 accountLedgerEntry.Transaction.SubType == TransactionSubType.MessagingArbitraryMessage))
            {
                return accountLedgerEntry.Transaction.Message?.MessageText;
            }
            var input = "[" + GetLedgerEntryType(accountLedgerEntry) + "]";
            return Regex.Replace(input, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }

        private static LedgerEntryType GetLedgerEntryType(AccountLedgerEntry accountLedgerEntry)
        {
            switch (accountLedgerEntry.EventType)
            {
                case "BLOCK_GENERATED":
                    return LedgerEntryType.BlockGenerated;
                case "REJECT_PHASED_TRANSACTION":
                    return LedgerEntryType.RejectPhasedTransaction;
                case "TRANSACTION_FEE":
                    return (LedgerEntryType)(int)accountLedgerEntry.Transaction.SubType;
                case "ORDINARY_PAYMENT":
                    return LedgerEntryType.OrdinaryPayment;
                case "ACCOUNT_INFO":
                    return LedgerEntryType.AccountInfo;
                case "ALIAS_ASSIGNMENT":
                    return LedgerEntryType.AliasAssignment;
                case "ALIAS_BUY":
                    return LedgerEntryType.AliasBuy;
                case "ALIAS_DELETE":
                    return LedgerEntryType.AliasDelete;
                case "ALIAS_SELL":
                    return LedgerEntryType.AliasSell;
                case "ARBITRARY_MESSAGE":
                    return LedgerEntryType.ArbitraryMessage;
                case "HUB_ANNOUNCEMENT":
                    return LedgerEntryType.HubAnnouncement;
                case "PHASING_VOTE_CASTING":
                    return LedgerEntryType.PhasingVoteCasting;
                case "POLL_CREATION":
                    return LedgerEntryType.PollCreation;
                case "VOTE_CASTING":
                    return LedgerEntryType.VoteCasting;
                case "ACCOUNT_PROPERTY":
                    return LedgerEntryType.AccountProperty;
                case "ACCOUNT_PROPERTY_DELETE":
                    return LedgerEntryType.AccountPropertyDelete;
                case "ASSET_ASK_ORDER_CANCELLATION":
                    return LedgerEntryType.AssetAskOrderCancellation;
                case "ASSET_ASK_ORDER_PLACEMENT":
                    return LedgerEntryType.AssetAskOrderPlacement;
                case "ASSET_BID_ORDER_CANCELLATION":
                    return LedgerEntryType.AssetBidOrderCancellation;
                case "ASSET_BID_ORDER_PLACEMENT":
                    return LedgerEntryType.AssetBidOrderPlacement;
                case "ASSET_DIVIDEND_PAYMENT":
                    return LedgerEntryType.AssetDividendPayment;
                case "ASSET_ISSUANCE":
                    return LedgerEntryType.AssetIssuance;
                case "ASSET_TRADE":
                    return LedgerEntryType.AssetTrade;
                case "ASSET_TRANSFER":
                    return LedgerEntryType.AssetTransfer;
                case "ASSET_DELETE":
                    return LedgerEntryType.AssetDelete;
                case "DIGITAL_GOODS_DELISTED":
                    return LedgerEntryType.DigitalGoodsDelisted;
                case "DIGITAL_GOODS_DELISTING":
                    return LedgerEntryType.DigitalGoodsDelisting;
                case "DIGITAL_GOODS_DELIVERY":
                    return LedgerEntryType.DigitalGoodsDelivery;
                case "DIGITAL_GOODS_FEEDBACK":
                    return LedgerEntryType.DigitalGoodsFeedback;
                case "DIGITAL_GOODS_LISTING":
                    return LedgerEntryType.DigitalGoodsListing;
                case "DIGITAL_GOODS_PRICE_CHANGE":
                    return LedgerEntryType.DigitalGoodsPriceChange;
                case "DIGITAL_GOODS_PURCHASE":
                    return LedgerEntryType.DigitalGoodsPurchase;
                case "DIGITAL_GOODS_PURCHASE_EXPIRED":
                    return LedgerEntryType.DigitalGoodsPurchaseExpired;
                case "DIGITAL_GOODS_QUANTITY_CHANGE":
                    return LedgerEntryType.DigitalGoodsQuantityChange;
                case "DIGITAL_GOODS_REFUND":
                    return LedgerEntryType.DigitalGoodsRefund;
                case "ACCOUNT_CONTROL_EFFECTIVE_BALANCE_LEASING":
                    return LedgerEntryType.AccountControlEffectiveBalanceLeasing;
                case "ACCOUNT_CONTROL_PHASING_ONLY":
                    return LedgerEntryType.AccountControlPhasingOnly;
                case "CURRENCY_DELETION":
                    return LedgerEntryType.CurrencyDeletion;
                case "CURRENCY_DISTRIBUTION":
                    return LedgerEntryType.CurrencyDistribution;
                case "CURRENCY_EXCHANGE":
                    return LedgerEntryType.CurrencyExchange;
                case "CURRENCY_EXCHANGE_BUY":
                    return LedgerEntryType.CurrencyExchangeBuy;
                case "CURRENCY_EXCHANGE_SELL":
                    return LedgerEntryType.CurrencyExchangeSell;
                case "CURRENCY_ISSUANCE":
                    return LedgerEntryType.CurrencyIssuance;
                case "CURRENCY_MINTING":
                    return LedgerEntryType.CurrencyMinting;
                case "CURRENCY_OFFER_EXPIRED":
                    return LedgerEntryType.CurrencyOfferExpired;
                case "CURRENCY_OFFER_REPLACED":
                    return LedgerEntryType.ShufflingReplaced;
                case "CURRENCY_PUBLISH_EXCHANGE_OFFER":
                    return LedgerEntryType.CurrencyPublishExchangeOffer;
                case "CURRENCY_RESERVE_CLAIM":
                    return LedgerEntryType.CurrencyReserveClaim;
                case "CURRENCY_RESERVE_INCREASE":
                    return LedgerEntryType.CurrencyReserveIncrease;
                case "CURRENCY_TRANSFER":
                    return LedgerEntryType.CurrencyTransfer;
                case "CURRENCY_UNDO_CROWDFUNDING":
                    return LedgerEntryType.CurrencyUndoCrowdfunding;
                case "TAGGED_DATA_UPLOAD":
                    return LedgerEntryType.TaggedDataUpload;
                case "TAGGED_DATA_EXTEND":
                    return LedgerEntryType.TaggedDataExtend;
                case "SHUFFLING_REGISTRATION":
                    return LedgerEntryType.ShufflingRegistration;
                case "SHUFFLING_PROCESSING":
                    return LedgerEntryType.ShufflingProcessing;
                case "SHUFFLING_CANCELLATION":
                    return LedgerEntryType.ShufflingCancellation;
                case "SHUFFLING_DISTRIBUTION":
                    return LedgerEntryType.ShufflingDistribution;
                default: throw new ArgumentException();
            }
        }
    }
}
