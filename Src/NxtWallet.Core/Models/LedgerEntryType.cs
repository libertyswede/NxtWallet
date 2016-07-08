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
        CurrencyOfferExpired = 1006,
        ShufflingRefund = 1007,
        ShufflingDistribution = 1008
    }

    // NRS Account Ledger Events

    //public enum LedgerEvent
    //{
    //    // Block and Transaction
    //    BLOCK_GENERATED(1, false), -------------------------------  Supported
    //    REJECT_PHASED_TRANSACTION(2, true),  ---------------------  Not supported
    //    TRANSACTION_FEE(50, true), -------------------------------  Supported
    //    // TYPE_PAYMENT                                             
    //    ORDINARY_PAYMENT(3, true), -------------------------------  Supported
    //    // TYPE_MESSAGING                                           
    //    ACCOUNT_INFO(4, true),     -------------------------------  Supported
    //    ALIAS_ASSIGNMENT(5, true), -------------------------------  Supported
    //    ALIAS_BUY(6, true),        -------------------------------  Supported
    //    ALIAS_DELETE(7, true),     -------------------------------  Supported
    //    ALIAS_SELL(8, true),       -------------------------------  Supported
    //    ARBITRARY_MESSAGE(9, true),  -----------------------------  Supported
    //    HUB_ANNOUNCEMENT(10, true),  -----------------------------  Not supported
    //    PHASING_VOTE_CASTING(11, true),  -------------------------  Supported
    //    POLL_CREATION(12, true),   -------------------------------  Supported
    //    VOTE_CASTING(13, true),    -------------------------------  Supported
    //    ACCOUNT_PROPERTY(56, true),  -----------------------------  Supported
    //    ACCOUNT_PROPERTY_DELETE(57, true),  ----------------------  Supported
    //    // TYPE_COLORED_COINS                                       
    //    ASSET_ASK_ORDER_CANCELLATION(14, true),  -----------------  Supported
    //    ASSET_ASK_ORDER_PLACEMENT(15, true),  --------------------  Supported
    //    ASSET_BID_ORDER_CANCELLATION(16, true),  -----------------  Supported
    //    ASSET_BID_ORDER_PLACEMENT(17, true),  --------------------  Supported
    //    ASSET_DIVIDEND_PAYMENT(18, true),  -----------------------  Supported
    //    ASSET_ISSUANCE(19, true),  -------------------------------  Supported
    //    ASSET_TRADE(20, true),     -------------------------------  Supported
    //    ASSET_TRANSFER(21, true),  -------------------------------  Supported
    //    ASSET_DELETE(49, true),    -------------------------------  Supported
    //    // TYPE_DIGITAL_GOODS                                       
    //    DIGITAL_GOODS_DELISTED(22, true), ------------------------  Supported
    //    DIGITAL_GOODS_DELISTING(23, true), -----------------------  Supported
    //    DIGITAL_GOODS_DELIVERY(24, true), ------------------------  Supported
    //    DIGITAL_GOODS_FEEDBACK(25, true), ------------------------  Supported
    //    DIGITAL_GOODS_LISTING(26, true),  ------------------------  Supported
    //    DIGITAL_GOODS_PRICE_CHANGE(27, true),  -------------------  Supported
    //    DIGITAL_GOODS_PURCHASE(28, true),  -----------------------  Supported
    //    DIGITAL_GOODS_PURCHASE_EXPIRED(29, true),  ---------------  Supported
    //    DIGITAL_GOODS_QUANTITY_CHANGE(30, true),  ----------------  Supported
    //    DIGITAL_GOODS_REFUND(31, true),  -------------------------  Supported
    //    // TYPE_ACCOUNT_CONTROL                                     
    //    ACCOUNT_CONTROL_EFFECTIVE_BALANCE_LEASING(32, true),  ----  Supported
    //    ACCOUNT_CONTROL_PHASING_ONLY(55, true),  -----------------  Supported
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
    //    SHUFFLING_REGISTRATION(51, true), ------------------------  Supported
    //    SHUFFLING_PROCESSING(52, true), --------------------------  Supported
    //    SHUFFLING_CANCELLATION(53, true), ------------------------  
    //    SHUFFLING_DISTRIBUTION(54, true); ------------------------  Supported
}
