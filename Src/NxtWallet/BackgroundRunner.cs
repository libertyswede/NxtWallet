using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using NxtLib;
using NxtWallet.Model;
using NxtWallet.ViewModel.Model;
using Transaction = NxtWallet.ViewModel.Model.Transaction;

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

        public event TransactionHandler TransactionConfirmationUpdated;
        public event TransactionHandler TransactionBalanceUpdated;
        public event TransactionHandler TransactionAdded;
        public event BalanceHandler BalanceUpdated;

        public BackgroundRunner(INxtServer nxtServer, ITransactionRepository transactionRepository,
            IBalanceCalculator balanceCalculator, IWalletRepository walletRepository,
            IContactRepository contactRepository, IAssetTracker assetTracker)
        {
            _nxtServer = nxtServer;
            _transactionRepository = transactionRepository;
            _balanceCalculator = balanceCalculator;
            _walletRepository = walletRepository;
            _contactRepository = contactRepository;
            _assetTracker = assetTracker;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var updatedTransactions = new List<Transaction>();
                try
                {
                    var knownTransactions = (await _transactionRepository.GetAllTransactionsAsync()).ToList();
                    var currentBlockId = await _nxtServer.GetCurrentBlockId();
                    var nxtTransactions = (await _nxtServer.GetTransactionsAsync()).ToList();
                    var balanceResult = await _nxtServer.GetBalanceAsync();

                    var newTransactions = nxtTransactions.Except(knownTransactions).ToList();
                    var allTransactions = newTransactions.Union(knownTransactions).ToList();

                    CheckDgsDeliveryTransactions(newTransactions, knownTransactions, updatedTransactions);
                    CheckSentDgsPurchaseTransactions(newTransactions);

                    var balancesMatch = _balanceCalculator.BalanceEqualsLastTransactionBalance(nxtTransactions,
                        knownTransactions, updatedTransactions, balanceResult);
                    if (!balancesMatch)
                    {
                        await CheckAssetTrades(knownTransactions, newTransactions);

                        // c. TODO: Fetch MS currency trades
                    }

                    await CheckSentDividendTransactions(newTransactions);

                    if (!balancesMatch && !_balanceCalculator.BalanceEqualsLastTransactionBalance(newTransactions, 
                        knownTransactions, updatedTransactions, balanceResult))
                    {
                        var blockReply = await _nxtServer.GetBlockAsync(_walletRepository.LastBalanceMatchBlockId);
                        await CheckReceivedDividendTransactions(knownTransactions, newTransactions, blockReply);
                        var forgeTransactions = await _nxtServer.GetForgingIncomeAsync(blockReply.Timestamp);
                        newTransactions.AddRange(forgeTransactions);
                        await CheckExpiredDgsPurchases(allTransactions, newTransactions);

                        if (_balanceCalculator.BalanceEqualsLastTransactionBalance(newTransactions,
                            knownTransactions, updatedTransactions, balanceResult))
                        {
                            await _walletRepository.UpdateLastBalanceMatchBlockIdAsync(currentBlockId);
                        }
                        else
                        {
                            // WTF?! Balances still don't match!!
                            throw new Exception("Fatal Fucking Error Baby!");
                        }
                    }
                    else
                    {
                        await _walletRepository.UpdateLastBalanceMatchBlockIdAsync(currentBlockId);
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