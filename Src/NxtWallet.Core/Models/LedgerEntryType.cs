namespace NxtWallet.Core.Models
{
    public enum LedgerEntryType
    {
        // Payment
        OrdinaryPayment,

        // Messaging
        ArbitraryMessage,
        AliasAssignment,
        PollCreation,
        VoteCasting,
        HubAnnouncement,
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
        AssetAskOrderPlacement,
        AssetBidOrderPlacement,
        AssetAskOrderCancellation,
        AssetBidOrderCancellation,
        AssetDividendPayment,
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
        AccountControlEffectiveBalanceLeasing,
        AccountControlPhasingOnly,

        // MonetarySystem
        CurrencyIssuance,
        CurrencyReserveIncrease,
        CurrencyReserveClaim,
        CurrencyTransfer,
        CurrencyPublishExchangeOffer,
        CurrencyExchangeBuy,
        CurrencyExchangeSell,
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

        // Technically not transactions, but account ledger events
        AssetTrade = 1001,
        BlockGenerated = 1002,
        DigitalGoodsPurchaseExpired = 1003,
        CurrencyUndoCrowdfunding = 1004,
        CurrencyExchange = 1005,
        CurrencyOfferExpired = 1006,
        ShufflingRefund = 1007,
        ShufflingReplaced = 1008,
        ShufflingDistribution = 1009,
        TransactionFee = 1010,
        RejectPhasedTransaction = 1011,
        CurrencyDistribution = 1012,
        DigitalGoodsDelisted = 1013
    }
}
