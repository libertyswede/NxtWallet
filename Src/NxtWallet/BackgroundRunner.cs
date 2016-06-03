using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using NxtLib;
using NxtLib.ServerInfo;
using NxtWallet.Model;
using NxtWallet.ViewModel.Model;
using Transaction = NxtWallet.ViewModel.Model.Transaction;
using TransactionType = NxtWallet.ViewModel.Model.TransactionType;
using NxtLib.Shuffling;

namespace NxtWallet
{
    public delegate void TransactionHandler(IBackgroundRunner sender, Transaction transaction);
    public delegate void BalanceHandler(IBackgroundRunner sender, string balance);

    public interface IBackgroundRunner
    {
        Task Run(CancellationToken token);

        event TransactionHandler TransactionConfirmationUpdated;
        event TransactionHandler TransactionBalanceUpdated;
        event TransactionHandler TransactionAdded;
        event BalanceHandler BalanceUpdated;
    }

    public class BackgroundRunner : IBackgroundRunner
    {
        public event TransactionHandler TransactionConfirmationUpdated;
        public event TransactionHandler TransactionBalanceUpdated;
        public event TransactionHandler TransactionAdded;
        public event BalanceHandler BalanceUpdated;

        private readonly INxtServer _nxtServer;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBalanceCalculator _balanceCalculator;
        private readonly IWalletRepository _walletRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IAssetTracker _assetTracker;
        private readonly IMsCurrencyTracker _msCurrencyTracker;

        /// <summary>
        /// Fetched from nxt server, could be both known and new transactions
        /// </summary>
        private List<Transaction> _nxtTransactions;

        /// <summary>
        /// All previously known transactions, fetched from local db
        /// </summary>
        private List<Transaction> _knownTransactions;

        /// <summary>
        /// Only previously unknown transactions
        /// </summary>
        private List<Transaction> _newTransactions;

        /// <summary>
        /// All transactions, new & Known transactions combined
        /// </summary>
        private List<Transaction> _allTransactions;

        /// <summary>
        /// Transactions that are updated this run
        /// </summary>
        private HashSet<Transaction> _updatedTransactions;

        /// <summary>
        /// Last block where the unconfirmed balance matched with sum of all known transactions
        /// </summary>
        private Block<ulong> _lastBalanceMatchBlock;

        /// <summary>
        /// Current unconfirmed balance from NXT server
        /// </summary>
        private long _unconfirmedBalanceNqt;

        /// <summary>
        /// Status fetched from NXT server
        /// </summary>
        private BlockchainStatus _blockchainStatus;

        public BackgroundRunner(INxtServer nxtServer, ITransactionRepository transactionRepository,
            IBalanceCalculator balanceCalculator, IWalletRepository walletRepository,
            IContactRepository contactRepository, IAssetTracker assetTracker, IMsCurrencyTracker msCurrencyTracker)
        {
            _nxtServer = nxtServer;
            _transactionRepository = transactionRepository;
            _balanceCalculator = balanceCalculator;
            _walletRepository = walletRepository;
            _contactRepository = contactRepository;
            _assetTracker = assetTracker;
            _msCurrencyTracker = msCurrencyTracker;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _updatedTransactions = new HashSet<Transaction>();
                try
                {
                    _lastBalanceMatchBlock = await _nxtServer.GetBlockAsync(_walletRepository.LastBalanceMatchBlockId);
                    _knownTransactions = (await _transactionRepository.GetAllTransactionsAsync()).ToList();
                    _blockchainStatus = await _nxtServer.GetBlockchainStatusAsync();
                    _nxtTransactions = (await _nxtServer.GetTransactionsAsync(_lastBalanceMatchBlock.Timestamp)).ToList();
                    _unconfirmedBalanceNqt = await _nxtServer.GetUnconfirmedNqtBalanceAsync();

                    _newTransactions = _nxtTransactions.Except(_knownTransactions).ToList();
                    _allTransactions = _newTransactions.Union(_knownTransactions).ToList();

                    CheckDgsDeliveryTransactions();
                    await CheckMsReserveIncreaseTransactions();
                    await CheckMsReserveClaimTransactions();
                    await CheckShufflingRegistrationTransactions();

                    if (!BalancesMatch())
                    {
                        await CheckAssetTrades();
                        await CheckMsExchanges();
                    }

                    await CheckSentDividendTransactions();
                    await CheckExpiredExchangeOffers();

                    if (!BalancesMatch())
                    {
                        await CheckReceivedDividendTransactions();
                        var forgeTransactions = await _nxtServer.GetForgingIncomeAsync(_lastBalanceMatchBlock.Timestamp);
                        _newTransactions.AddRange(forgeTransactions);
                        await CheckExpiredDgsPurchases();
                        await CheckMsUndoCrowdfundingTransaction();
                        await CheckFinishedShufflingTransactions();
                        var deletedTransactions = await RemovePreviouslyUnconfirmedNowRemovedTransactions();

                        if (BalancesMatch(deletedTransactions))
                        {
                            await _walletRepository.UpdateLastBalanceMatchBlockIdAsync(_blockchainStatus.LastBlockId);
                        }
                        else
                        {
                            // WTF?! Balances still don't match!!
                            throw new Exception("Fatal Fucking Error Baby!");
                        }
                    }
                    else
                    {
                        await _walletRepository.UpdateLastBalanceMatchBlockIdAsync(_blockchainStatus.LastBlockId);
                    }

                    GetTransactionsWithUpdatedConfirmation()
                        .Union(await HandleNewTransactions())
                        .ToList()
                        .ForEach(t => _updatedTransactions.Add(t));
                    await HandleUpdatedTransactions();
                    await HandleBalance();
                    await _assetTracker.SaveOwnerships();

                    await Task.Delay(_walletRepository.SleepTime, token);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        private async Task CheckShufflingRegistrationTransactions()
        {
            var newRegistrationTransactions = _newTransactions.Where(t => t.TransactionType == TransactionType.ShufflingRegistration)
                .Cast<ShufflingRegistrationTransaction>()
                .ToList();

            foreach (var newRegistrationTransaction in newRegistrationTransactions)
            {
                var shuffling = await _nxtServer.GetShuffling((ulong)newRegistrationTransaction.ShufflingId);
                newRegistrationTransaction.NqtAmount = shuffling.Amount.Nqt;
            }
        }

        private async Task<List<Transaction>> RemovePreviouslyUnconfirmedNowRemovedTransactions()
        {
            var knownUnconfirmed = _knownTransactions.Where(t => !t.IsConfirmed).ToList();
            var unconfirmedToRemove = knownUnconfirmed.Where(ku => !_nxtTransactions.Exists(t => t.Equals(ku))).ToList();
            foreach (var unconfirmed in unconfirmedToRemove)
            {
                _knownTransactions.Remove(unconfirmed);
                await _transactionRepository.RemoveTransactionAsync(unconfirmed);
            }
            return unconfirmedToRemove;
        }

        private async Task CheckMsExchanges()
        {
            await _msCurrencyTracker.CheckMsExchanges(_allTransactions, _newTransactions, _updatedTransactions);
        }

        private async Task CheckExpiredExchangeOffers()
        {
            await _msCurrencyTracker.ExpireExchangeOffers(_allTransactions, _newTransactions, _updatedTransactions, _blockchainStatus.NumberOfBlocks - 1);
        }

        private async Task CheckMsReserveClaimTransactions()
        {
            foreach (var claimTransaction in _newTransactions.Where(t => t.TransactionType == TransactionType.ReserveClaim))
            {
                var attachment = (MonetarySystemReserveClaimAttachment) claimTransaction.Attachment;
                var currency = await _nxtServer.GetCurrencyAsync(attachment.CurrencyId);
                claimTransaction.NqtAmount = attachment.Units/(long) Math.Pow(10, currency.Decimals)*100000000;
            }
        }

        private async Task CheckMsReserveIncreaseTransactions()
        {
            foreach (var reserveTransaction in _newTransactions.Where(t => t.TransactionType == TransactionType.ReserveIncrease)
                        .Cast<MsReserveIncreaseTransaction>())
            {
                var attachment = (MonetarySystemReserveIncreaseAttachment) reserveTransaction.Attachment;
                try
                {
                    var createCurrencyTransaction = await _nxtServer.GetTransactionAsync(attachment.CurrencyId);
                    var issuanceAttachment = (MonetarySystemCurrencyIssuanceAttachment)createCurrencyTransaction.Attachment;
                    reserveTransaction.IssuanceHeight = issuanceAttachment.IssuanceHeight;
                    reserveTransaction.NqtAmount = issuanceAttachment.ReserveSupply*attachment.AmountPerUnit.Nqt;
                }
                catch (NxtException e)
                {
                    if (e.Message != "Unknown currency")
                    {
                        throw;
                    }
                }
            }
        }

        private async Task CheckMsUndoCrowdfundingTransaction()
        {
            var undoTransactions = _allTransactions
                .Where(t => t.TransactionType == TransactionType.CurrencyUndoCrowdfunding)
                .Cast<MsUndoCrowdfundingTransaction>()
                .ToList();

            var reserveTransactions = _allTransactions.Where(t => t.TransactionType == TransactionType.ReserveIncrease)
                            .Cast<MsReserveIncreaseTransaction>()
                            .Where(t => t.IssuanceHeight < _blockchainStatus.NumberOfBlocks);

            foreach (var reserveTransaction in reserveTransactions)
            {
                try
                {
                    await _nxtServer.GetCurrencyAsync((ulong) reserveTransaction.CurrencyId);
                    continue;
                }
                catch (NxtException e)
                {
                    if (e.Message != "Unknown currency")
                    {
                        throw;
                    }
                }

                if (!undoTransactions.Any(t => t.ReserveIncreaseNxtId == (long) reserveTransaction.NxtId))
                {
                    var block = await _nxtServer.GetBlockAsync(reserveTransaction.IssuanceHeight);

                    var transaction = new MsUndoCrowdfundingTransaction
                    {
                        TransactionType = TransactionType.CurrencyUndoCrowdfunding,
                        NqtAmount = reserveTransaction.NqtAmount,
                        AccountFrom = Transaction.GeneratedFromAddress,
                        AccountTo = _walletRepository.NxtAccount.AccountRs,
                        NxtId = null,
                        Height = reserveTransaction.IssuanceHeight,
                        Timestamp = block.Timestamp,
                        Message = "[Currency Crowdfunding Cancellation]",
                        ReserveIncreaseNxtId = (long)(reserveTransaction.NxtId ?? 0),
                        IsConfirmed = true,
                        UserIsTransactionSender = false,
                        UserIsTransactionRecipient = true
                    };

                    _newTransactions.Add(transaction);
                }
            }
        }

        private bool BalancesMatch()
        {
            return BalancesMatch(new List<Transaction>());
        }

        private bool BalancesMatch(IReadOnlyList<Transaction> removedTransactions)
        {
            GetTransactionsWithUpdatedConfirmation().ForEach(t => _updatedTransactions.Add(t));
            var balancesMatch = _balanceCalculator.BalanceEqualsLastTransactionBalance(_newTransactions,
                _knownTransactions, _updatedTransactions, removedTransactions, _unconfirmedBalanceNqt);
            return balancesMatch;
        }

        private async Task CheckAssetTrades()
        {
            var tradesResult = (await _nxtServer.GetAssetTradesAsync(_walletRepository.LastAssetTrade)).ToList();
            var newTrades = tradesResult.Except(_knownTransactions).ToList();
            _newTransactions.AddRange(newTrades);

            if (tradesResult.Any())
            {
                await _walletRepository.UpdateLastAssetTrade(tradesResult.Max(t => t.Timestamp).AddSeconds(1));
            }
        }

        private async Task CheckFinishedShufflingTransactions()
        {
            var shufflingCreations = _allTransactions
                .Where(t => t.TransactionType == TransactionType.ShufflingCreation)
                .Cast<ShufflingCreationTransaction>()
                .Where(t => !t.Done)
                .ToList();

            var shufflingRegistrations = _allTransactions
                .Where(t => t.TransactionType == TransactionType.ShufflingRegistration)
                .Cast<ShufflingRegistrationTransaction>()
                .Where(t => !t.Done)
                .ToList();

            foreach (var shufflingCreation in shufflingCreations)
            {
                var shuffling = await _nxtServer.GetShuffling(shufflingCreation.NxtId.Value);
                if (shuffling.Stage == ShufflingStage.Done)
                {
                    shufflingCreation.Done = true;
                    _updatedTransactions.Add(shufflingCreation);
                }
                else if (shuffling.Stage == ShufflingStage.Cancelled && shuffling.ParticipantCount != shuffling.RegistrantCount)
                {
                    var block = await _nxtServer.GetBlockAsync(shufflingCreation.Height + shufflingCreation.RegistrationPeriod - 1);
                    var refundTransaction = new ShufflingRefundTransaction
                    {
                        AccountFrom = Transaction.GeneratedFromAddress,
                        AccountTo = _walletRepository.NxtAccount.AccountRs,
                        Height = block.Height,
                        IsConfirmed = true,
                        Message = "[Shuffling Refund]",
                        NqtAmount = shufflingCreation.NqtAmount,
                        NqtFee = 0,
                        NxtId = null,
                        ShufflingId = (long)shufflingCreation.NxtId.Value,
                        Timestamp = block.Timestamp,
                        TransactionType = TransactionType.ShufflingRefund,
                        UserIsTransactionRecipient = true,
                        UserIsTransactionSender = false,
                    };
                    _newTransactions.Add(refundTransaction);

                    shufflingCreation.Done = true;
                    _updatedTransactions.Add(shufflingCreation);
                }
            }
        }

        private async Task CheckExpiredDgsPurchases()
        {
            var undeliveredPurchases = _allTransactions
                .Where(t => t.TransactionType == TransactionType.DigitalGoodsPurchase && t.UserIsTransactionSender)
                .Select(t => (DgsPurchaseTransaction) t)
                .Where(t => !t.DeliveryTransactionNxtId.HasValue)
                .ToList();

            var expiredTransactions = _allTransactions.Where(t => t.TransactionType == TransactionType.DigitalGoodsPurchaseExpired)
                    .Cast<DgsPurchaseExpiredTransaction>()
                    .ToList();

            foreach (var undeliveredPurchase in undeliveredPurchases)
            {
                if (expiredTransactions.Any(t => undeliveredPurchase.NxtId.HasValue && t.PurchaseTransactionNxtId == (long) undeliveredPurchase.NxtId.Value))
                {
                    continue;
                }

                var isPurchaseExpired = await _nxtServer.GetIsPurchaseExpired(undeliveredPurchase.NxtId.Value);

                if (isPurchaseExpired)
                {
                    var expiredTransaction = new DgsPurchaseExpiredTransaction
                    {
                        TransactionType = TransactionType.DigitalGoodsPurchaseExpired,
                        NxtId = null,
                        AccountFrom = undeliveredPurchase.AccountTo,
                        Height = undeliveredPurchase.Height,
                        IsConfirmed = true,
                        NqtAmount = undeliveredPurchase.NqtAmount,
                        AccountTo = undeliveredPurchase.AccountFrom,
                        Message = "[Digital Goods Purchase Expired]",
                        Timestamp = undeliveredPurchase.DeliveryDeadlineTimestamp,
                        PurchaseTransactionNxtId = (long) undeliveredPurchase.NxtId,
                        NqtFee = 0
                    };
                    expiredTransaction.UserIsTransactionSender = _walletRepository.NxtAccount.AccountRs.Equals(expiredTransaction.AccountFrom);
                    expiredTransaction.UserIsTransactionRecipient = _walletRepository.NxtAccount.AccountRs.Equals(expiredTransaction.AccountTo);

                    _newTransactions.Add(expiredTransaction);
                }
            }
        }

        private async Task CheckReceivedDividendTransactions()
        {
            var assets = (await _assetTracker.GetOwnedAssetsSince(_lastBalanceMatchBlock.Height))
                .Where(a => a.Account != _walletRepository.NxtAccount.AccountRs);

            foreach (var asset in assets)
            {
                var dividendTransactions = await _nxtServer.GetDividendTransactionsAsync(asset.Account, _lastBalanceMatchBlock.Timestamp);
                foreach (var dividendTransaction in dividendTransactions.Except(_knownTransactions))
                {
                    var attachment = (ColoredCoinsDividendPaymentAttachment) dividendTransaction.Attachment;
                    var ownership = await _assetTracker.GetOwnership(attachment.AssetId, attachment.Height);
                    if (ownership?.BalanceQnt > 0)
                    {
                        dividendTransaction.NqtAmount = attachment.AmountPerQnt.Nqt*ownership.BalanceQnt;
                        _newTransactions.Add(dividendTransaction);
                    }
                }
            }
        }

        private void CheckDgsDeliveryTransactions()
        {
            foreach (var deliveryTransaction in _newTransactions.Where(t => t.TransactionType == TransactionType.DigitalGoodsDelivery).ToList())
            {
                // Find & update the payment transaction
                var deliveryAttachment = (DigitalGoodsDeliveryAttachment) deliveryTransaction.Attachment;
                var purchaseTransaction = (DgsPurchaseTransaction) _knownTransactions
                    .SingleOrDefault(t => t.NxtId == deliveryAttachment.Purchase);

                if (purchaseTransaction != null)
                {
                    _updatedTransactions.Add(purchaseTransaction);
                }
                else
                {
                    purchaseTransaction = (DgsPurchaseTransaction) _newTransactions.Single(t => t.NxtId == deliveryAttachment.Purchase);
                }
                purchaseTransaction.DeliveryTransactionNxtId = (long) deliveryTransaction.NxtId;

                // If I am delivering, update the amount with what the customer is paying
                if (deliveryTransaction.UserIsTransactionSender)
                {
                    var purchaseAttachment = (DigitalGoodsPurchaseAttachment) purchaseTransaction.Attachment;
                    var amount = purchaseAttachment.Price.Nqt*purchaseAttachment.Quantity -
                                 deliveryAttachment.Discount.Nqt;
                    deliveryTransaction.NqtAmount = amount;
                }
            }
        }

        private async Task CheckSentDividendTransactions()
        {
            await _assetTracker.UpdateAssetOwnership(_newTransactions);
            foreach (var dividendTransaction in _newTransactions.Where(t => t.TransactionType == TransactionType.DividendPayment && t.UserIsTransactionSender).ToList())
            {
                var attachment = (ColoredCoinsDividendPaymentAttachment) dividendTransaction.Attachment;
                var myOwnership = await _assetTracker.GetOwnership(attachment.AssetId, attachment.Height);
                var quantityQnt = await _assetTracker.GetAssetQuantity(attachment.AssetId, attachment.Height);

                var shareholdersQnt = quantityQnt - myOwnership.BalanceQnt;
                var expenseNqt = attachment.AmountPerQnt.Nqt*shareholdersQnt;
                dividendTransaction.NqtAmount = expenseNqt;
            }
        }

        private async Task HandleUpdatedTransactions()
        {
            await _transactionRepository.UpdateTransactionsAsync(_updatedTransactions.Distinct());
            foreach (var updatedTransaction in _updatedTransactions)
            {
                OnTransactionConfirmationUpdated(updatedTransaction);
            }
        }

        private async Task<IEnumerable<Transaction>> HandleNewTransactions()
        {
            var updated = new List<Transaction>();
            if (_newTransactions.Any())
            {
                await UpdateTransactionContacts();
                var allTransactions = _knownTransactions.Union(_newTransactions).OrderBy(t => t.Timestamp).ToList();
                updated = _balanceCalculator.Calculate(_newTransactions, new List<Transaction>(), allTransactions).ToList();
                await _transactionRepository.SaveTransactionsAsync(_newTransactions);

                _newTransactions.ForEach(OnTransactionAdded);
            }
            return updated;
        }

        private async Task UpdateTransactionContacts()
        {
            var accountsRs =
                _newTransactions.Select(t => t.AccountFrom).Union(_newTransactions.Select(t => t.AccountTo)).Distinct();
            var contacts = (await _contactRepository.GetContactsAsync(accountsRs)).ToDictionary(contact => contact.NxtAddressRs);
            _newTransactions.ForEach(t => t.UpdateWithContactInfo(contacts));
        }

        private async Task HandleBalance()
        {
            var unconfirmedBalanceNqt = _newTransactions.Union(_knownTransactions)
                .Where(t => t.UserIsTransactionRecipient && !t.IsConfirmed)
                .Sum(t => t.NqtAmount);

            var balanceNqt = unconfirmedBalanceNqt + _unconfirmedBalanceNqt;
            var balance = balanceNqt.NqtToNxt().ToFormattedString();
            if (balance != _walletRepository.Balance)
            {
                await _walletRepository.UpdateBalanceAsync(balance);
                OnBalanceUpdated(balance);
            }
        }

        private List<Transaction> GetTransactionsWithUpdatedConfirmation()
        {
            var updatedTransactions = _knownTransactions
                .Where(t => t.IsConfirmed == false && _nxtTransactions.Contains(t))
                .Except(_nxtTransactions.Where(t => t.IsConfirmed == false))
                .Except(_newTransactions)
                .ToList();

            foreach (var updatedTransaction in updatedTransactions)
            {
                var nxtTransaction = _nxtTransactions.Single(t => t.Equals(updatedTransaction));
                updatedTransaction.IsConfirmed = true;
                updatedTransaction.Height = nxtTransaction.Height;
            }
            
            return updatedTransactions;
        }

        protected virtual void OnTransactionConfirmationUpdated(Transaction transaction)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => TransactionConfirmationUpdated?.Invoke(this, transaction));
        }

        protected virtual void OnTransactionBalanceUpdated(Transaction transaction)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => TransactionBalanceUpdated?.Invoke(this, transaction));
        }

        protected virtual void OnTransactionAdded(Transaction transaction)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => TransactionAdded?.Invoke(this, transaction));
        }

        protected virtual void OnBalanceUpdated(string balance)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => BalanceUpdated?.Invoke(this, balance));
        }
    }
}