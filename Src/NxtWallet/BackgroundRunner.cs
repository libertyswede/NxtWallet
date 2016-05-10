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

namespace NxtWallet
{
    public interface IBackgroundRunner
    {
        Task Run(CancellationToken token);

        event TransactionHandler TransactionConfirmationUpdated;
        event TransactionHandler TransactionBalanceUpdated;
        event TransactionHandler TransactionAdded;
        event BalanceHandler BalanceUpdated;
    }

    public delegate void TransactionHandler(IBackgroundRunner sender, Transaction transaction);
    public delegate void BalanceHandler(IBackgroundRunner sender, string balance);

    public class BackgroundRunner : IBackgroundRunner
    {
        private readonly INxtServer _nxtServer;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBalanceCalculator _balanceCalculator;
        private readonly IWalletRepository _walletRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IAssetTracker _assetTracker;
        private readonly IMsCurrencyTracker _msCurrencyTracker;

        public event TransactionHandler TransactionConfirmationUpdated;
        public event TransactionHandler TransactionBalanceUpdated;
        public event TransactionHandler TransactionAdded;
        public event BalanceHandler BalanceUpdated;

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
                var updatedTransactions = new List<Transaction>();
                try
                {
                    var knownTransactions = (await _transactionRepository.GetAllTransactionsAsync()).ToList();
                    var blockchainStatus = await _nxtServer.GetCurrentBlockId();
                    var nxtTransactions = (await _nxtServer.GetTransactionsAsync()).ToList();
                    var balanceResult = await _nxtServer.GetBalanceAsync();

                    var newTransactions = nxtTransactions.Except(knownTransactions).ToList();
                    var allTransactions = newTransactions.Union(knownTransactions).ToList();

                    CheckDgsRefundTransactions(newTransactions);
                    CheckDgsDeliveryTransactions(newTransactions, knownTransactions, updatedTransactions);
                    CheckSentDgsPurchaseTransactions(newTransactions);
                    await CheckMsReserveIncreaseTransactions(newTransactions);
                    await CheckMsReserveClaimTransactions(newTransactions);

                    var balancesMatch = BalancesMatch(updatedTransactions, knownTransactions, nxtTransactions, newTransactions, balanceResult);
                    if (!balancesMatch)
                    {
                        await CheckAssetTrades(knownTransactions, newTransactions);
                        await CheckMsExchanges(allTransactions, newTransactions, updatedTransactions);
                    }

                    await CheckSentDividendTransactions(newTransactions);
                    await CheckExpiredExchangeOffers(newTransactions, allTransactions, updatedTransactions, blockchainStatus.NumberOfBlocks-1);

                    if (!balancesMatch && !BalancesMatch(updatedTransactions, knownTransactions, nxtTransactions, newTransactions, balanceResult))
                    {
                        var blockReply = await _nxtServer.GetBlockAsync(_walletRepository.LastBalanceMatchBlockId);
                        await CheckReceivedDividendTransactions(knownTransactions, newTransactions, blockReply);
                        var forgeTransactions = await _nxtServer.GetForgingIncomeAsync(blockReply.Timestamp);
                        newTransactions.AddRange(forgeTransactions);
                        await CheckExpiredDgsPurchases(allTransactions, newTransactions);
                        await CheckMsUndoCrowdfundingTransaction(knownTransactions, blockchainStatus, newTransactions);
                        
                        if (BalancesMatch(updatedTransactions, knownTransactions, nxtTransactions, newTransactions, balanceResult))
                        {
                            await _walletRepository.UpdateLastBalanceMatchBlockIdAsync(blockchainStatus.LastBlockId);
                        }
                        else
                        {
                            // WTF?! Balances still don't match!!
                            throw new Exception("Fatal Fucking Error Baby!");
                        }
                    }
                    else
                    {
                        await _walletRepository.UpdateLastBalanceMatchBlockIdAsync(blockchainStatus.LastBlockId);
                    }
                    
                    updatedTransactions.AddRange(GetTransactionsWithUpdatedConfirmation(knownTransactions, nxtTransactions, newTransactions));
                    updatedTransactions.AddRange(await HandleNewTransactions(newTransactions, knownTransactions));
                    await HandleUpdatedTransactions(updatedTransactions);
                    await HandleBalance(balanceResult, newTransactions, knownTransactions);
                    await _assetTracker.SaveOwnerships();

                    await Task.Delay(_walletRepository.SleepTime, token);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        private async Task CheckMsExchanges(IReadOnlyCollection<Transaction> allTransactions,
            List<Transaction> newTransactions, ICollection<Transaction> updatedTransactions)
        {
            await _msCurrencyTracker.CheckMsExchanges(allTransactions, newTransactions, updatedTransactions);
        }

        private async Task CheckExpiredExchangeOffers(List<Transaction> newTransactions, List<Transaction> allTransactions, 
            List<Transaction> updatedTransactions, int currentHeight)
        {
            await _msCurrencyTracker.ExpireExchangeOffers(allTransactions, newTransactions, updatedTransactions, currentHeight);
        }

        private async Task CheckMsReserveClaimTransactions(IEnumerable<Transaction> newTransactions)
        {
            foreach (var claimTransaction in newTransactions.Where(t => t.TransactionType == TransactionType.ReserveClaim))
            {
                var attachment = (MonetarySystemReserveClaimAttachment) claimTransaction.Attachment;
                var currency = await _nxtServer.GetCurrencyAsync(attachment.CurrencyId);
                claimTransaction.NqtAmount = attachment.Units/(long) Math.Pow(10, currency.Decimals)*100000000;
            }
        }

        private async Task CheckMsReserveIncreaseTransactions(IEnumerable<Transaction> newTransactions)
        {
            foreach (var reserveTransaction in newTransactions.Where(t => t.TransactionType == TransactionType.ReserveIncrease)
                        .Cast<MsReserveIncreaseTransaction>())
            {
                var attachment = (MonetarySystemReserveIncreaseAttachment) reserveTransaction.Attachment;
                try
                {
                    var currency = await _nxtServer.GetCurrencyAsync(attachment.CurrencyId);
                    reserveTransaction.IssuanceHeight = currency.IssuanceHeight;
                    reserveTransaction.NqtAmount = currency.ReserveSupply*attachment.AmountPerUnit.Nqt;
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

        private async Task CheckMsUndoCrowdfundingTransaction(IEnumerable<Transaction> knownTransactions, 
            BlockchainStatus blockchainStatus, ICollection<Transaction> newTransactions)
        {
            foreach (var reserveTransaction in knownTransactions.Where(t => t.TransactionType == TransactionType.ReserveIncrease)
                            .Cast<MsReserveIncreaseTransaction>()
                            .Where(t => t.IssuanceHeight < blockchainStatus.NumberOfBlocks))
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

                var block = await _nxtServer.GetBlockAsync(reserveTransaction.IssuanceHeight);

                var transaction = new MsUndoCrowdfundingTransaction
                {
                    TransactionType = TransactionType.CurrencyUndoCrowdfunding,
                    NqtAmount = reserveTransaction.NqtAmount,
                    AccountFrom = "System Generated",
                    AccountTo = _walletRepository.NxtAccount.AccountRs,
                    NxtId = null,
                    Height = reserveTransaction.IssuanceHeight,
                    Timestamp = block.Timestamp,
                    Message = "[Currency Crowdfunding Cancellation]",
                    ReserveIncreaseNxtId = (long) (reserveTransaction.NxtId ?? 0),
                    IsConfirmed = true,
                    UserIsTransactionSender = false,
                    UserIsTransactionRecipient = true
                };

                newTransactions.Add(transaction);
            }
        }

        private static void CheckDgsRefundTransactions(List<Transaction> newTransactions)
        {
            foreach (
                var refundTransaction in newTransactions.Where(t => t.TransactionType == TransactionType.DigitalGoodsRefund))
            {
                var attachment = (DigitalGoodsRefundAttachment) refundTransaction.Attachment;
                refundTransaction.NqtAmount += attachment.Refund.Nqt;
            }
        }

        private bool BalancesMatch(List<Transaction> updatedTransactions, IReadOnlyList<Transaction> knownTransactions,
            IReadOnlyList<Transaction> nxtTransactions, IReadOnlyList<Transaction> newTransactions,
            long balanceResult)
        {
            updatedTransactions.AddRange(GetTransactionsWithUpdatedConfirmation(knownTransactions,
                nxtTransactions, newTransactions));
            var balancesMatch = _balanceCalculator.BalanceEqualsLastTransactionBalance(newTransactions,
                knownTransactions, updatedTransactions, balanceResult);
            return balancesMatch;
        }

        private async Task CheckAssetTrades(IEnumerable<Transaction> knownTransactions, List<Transaction> newTransactions)
        {
            var tradesResult = (await _nxtServer.GetAssetTradesAsync(_walletRepository.LastAssetTrade)).ToList();
            var newTrades = tradesResult.Except(knownTransactions).ToList();
            newTransactions.AddRange(newTrades);

            if (tradesResult.Any())
            {
                await _walletRepository.UpdateLastAssetTrade(tradesResult.Max(t => t.Timestamp).AddSeconds(1));
            }
        }

        private async Task CheckExpiredDgsPurchases(IReadOnlyCollection<Transaction> allTransactions, ICollection<Transaction> newTransactions)
        {
            var undeliveredPurchases = allTransactions
                .Where(t => t.TransactionType == TransactionType.DigitalGoodsPurchase && t.UserIsTransactionSender)
                .Select(t => (DgsPurchaseTransaction) t)
                .Where(t => !t.DeliveryTransactionNxtId.HasValue)
                .ToList();

            var expiredTransactions = allTransactions.Where(t => t.TransactionType == TransactionType.DigitalGoodsPurchaseExpired)
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

                    newTransactions.Add(expiredTransaction);
                }
            }
        }

        private async Task CheckReceivedDividendTransactions(
            IReadOnlyCollection<Transaction> knownTransactions, ICollection<Transaction> newTransactions,
            Block<ulong> block)
        {
            var assets = (await _assetTracker.GetOwnedAssetsSince(block.Height))
                .Where(a => a.Account != _walletRepository.NxtAccount.AccountRs);

            foreach (var asset in assets)
            {
                var dividendTransactions = await _nxtServer.GetDividendTransactionsAsync(asset.Account, block.Timestamp);
                foreach (var dividendTransaction in dividendTransactions.Except(knownTransactions))
                {
                    var attachment = (ColoredCoinsDividendPaymentAttachment) dividendTransaction.Attachment;
                    var ownership = await _assetTracker.GetOwnership(attachment.AssetId, attachment.Height);
                    if (ownership?.BalanceQnt > 0)
                    {
                        dividendTransaction.NqtAmount = attachment.AmountPerQnt.Nqt*ownership.BalanceQnt;
                        newTransactions.Add(dividendTransaction);
                    }
                }
            }
        }

        private static void CheckDgsDeliveryTransactions(List<Transaction> newTransactions, List<Transaction> knownTransactions, List<Transaction> updatedTransactions)
        {
            foreach (var deliveryTransaction in newTransactions.Where(t => t.TransactionType == TransactionType.DigitalGoodsDelivery).ToList())
            {
                // Find & update the payment transaction
                var deliveryAttachment = (DigitalGoodsDeliveryAttachment) deliveryTransaction.Attachment;
                var purchaseTransaction = (DgsPurchaseTransaction) knownTransactions
                    .SingleOrDefault(t => t.NxtId == deliveryAttachment.Purchase);

                if (purchaseTransaction != null)
                {
                    updatedTransactions.Add(purchaseTransaction);
                }
                else
                {
                    purchaseTransaction = (DgsPurchaseTransaction) newTransactions.Single(t => t.NxtId == deliveryAttachment.Purchase);
                }
                purchaseTransaction.DeliveryTransactionNxtId = (long) (deliveryTransaction.NxtId ?? 0);

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

        private static void CheckSentDgsPurchaseTransactions(IEnumerable<Transaction> newTransactions)
        {
            foreach (var transaction in newTransactions.Where(t => t.TransactionType == TransactionType.DigitalGoodsPurchase && t.UserIsTransactionSender))
            {
                var attachment = (DigitalGoodsPurchaseAttachment) transaction.Attachment;
                transaction.NqtAmount += attachment.Price.Nqt*attachment.Quantity;
            }
        }

        private async Task CheckSentDividendTransactions(List<Transaction> newTransactions)
        {
            await _assetTracker.UpdateAssetOwnership(newTransactions);
            foreach (var dividendTransaction in newTransactions.Where(t => t.TransactionType == TransactionType.DividendPayment && t.UserIsTransactionSender).ToList())
            {
                var attachment = (ColoredCoinsDividendPaymentAttachment) dividendTransaction.Attachment;
                var myOwnership = await _assetTracker.GetOwnership(attachment.AssetId, attachment.Height);
                var quantityQnt = await _assetTracker.GetAssetQuantity(attachment.AssetId, attachment.Height);

                var shareholdersQnt = quantityQnt - myOwnership.BalanceQnt;
                var expenseNqt = attachment.AmountPerQnt.Nqt*shareholdersQnt;
                dividendTransaction.NqtAmount = expenseNqt;
            }
        }

        private async Task HandleUpdatedTransactions(List<Transaction> updatedTransactions)
        {
            await _transactionRepository.UpdateTransactionsAsync(updatedTransactions.Distinct());
            updatedTransactions.ForEach(OnTransactionConfirmationUpdated);
        }

        private async Task<IEnumerable<Transaction>> HandleNewTransactions(List<Transaction> newTransactions, IEnumerable<Transaction> knownTransactions)
        {
            var updated = new List<Transaction>();
            if (newTransactions.Any())
            {
                await UpdateTransactionContacts(newTransactions);
                var allTransactions = knownTransactions.Union(newTransactions).OrderBy(t => t.Timestamp).ToList();
                updated = _balanceCalculator.Calculate(newTransactions, allTransactions).ToList();
                await _transactionRepository.SaveTransactionsAsync(newTransactions);

                newTransactions.ForEach(OnTransactionAdded);
            }
            return updated;
        }

        private async Task UpdateTransactionContacts(List<Transaction> newTransactions)
        {
            var accountsRs =
                newTransactions.Select(t => t.AccountFrom).Union(newTransactions.Select(t => t.AccountTo)).Distinct();
            var contacts = (await _contactRepository.GetContactsAsync(accountsRs)).ToDictionary(contact => contact.NxtAddressRs);
            newTransactions.ForEach(t => t.UpdateWithContactInfo(contacts));
        }

        private async Task HandleBalance(long confirmedBalanceNqt, IEnumerable<Transaction> newTransactions, List<Transaction> knownTransactions)
        {
            var unconfirmedBalanceNqt = newTransactions.Union(knownTransactions)
                .Where(t => t.UserIsTransactionRecipient && !t.IsConfirmed)
                .Sum(t => t.NqtAmount);

            var balanceNqt = unconfirmedBalanceNqt + confirmedBalanceNqt;
            var balance = balanceNqt.NqtToNxt().ToFormattedString();
            if (balance != _walletRepository.Balance)
            {
                await _walletRepository.UpdateBalanceAsync(balance);
                OnBalanceUpdated(balance);
            }
        }

        private static List<Transaction> GetTransactionsWithUpdatedConfirmation(IEnumerable<Transaction> knownTransactions, 
            IEnumerable<Transaction> nxtTransactions, IEnumerable<Transaction> newTransactions)
        {
            var updatedTransactions = knownTransactions
                .Where(t => t.IsConfirmed == false)
                .Except(nxtTransactions.Where(t => t.IsConfirmed == false))
                .Except(newTransactions)
                .ToList();

            updatedTransactions.ForEach(t => t.IsConfirmed = true);
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