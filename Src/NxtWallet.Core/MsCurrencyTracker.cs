using NxtWallet.Repositories.Model;
using NxtWallet.Core.ViewModel.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace NxtWallet.Core
{
    public interface IMsCurrencyTracker
    {
        Task CheckMsExchanges(IReadOnlyCollection<Transaction> allTransactions, List<Transaction> newTransactions,
            HashSet<Transaction> updatedTransactions);
        Task ExpireExchangeOffers(List<Transaction> allTransactions, List<Transaction> newTransactions,
            HashSet<Transaction> updatedTransactions, int currentHeight);
    }

    public class MsCurrencyTracker : IMsCurrencyTracker
    {
        private readonly INxtServer _nxtServer;
        private IWalletRepository _walletRepository;

        public MsCurrencyTracker(INxtServer nxtServer, IWalletRepository walletRepository)
        {
            _nxtServer = nxtServer;
            _walletRepository = walletRepository;
        }

        public async Task CheckMsExchanges(IReadOnlyCollection<Transaction> allTransactions, List<Transaction> newTransactions,
            HashSet<Transaction> updatedTransactions)
        {
            var exchangeTransactions = await GetNewExchangeTransactions();
            var offerTransactions = allTransactions.Where(t => t.TransactionType == TransactionType.PublishExchangeOffer)
                .Cast<MsPublishExchangeOfferTransaction>()
                .ToList();

            foreach (var exchangeTransaction in exchangeTransactions)
            {
                exchangeTransaction.NxtId = (ulong) exchangeTransaction.TransactionNxtId;
                var offerTransaction = offerTransactions.SingleOrDefault(t => t.NxtId == (ulong) exchangeTransaction.OfferNxtId);
                if (offerTransaction != null)
                {
                    // I am selling currency through ExchangeOffer
                    if (exchangeTransaction.UserIsAmountRecipient)
                    {
                        offerTransaction.SellLimit -= exchangeTransaction.Units;
                        offerTransaction.SellSupply -= exchangeTransaction.Units;
                        var excess = Math.Max(offerTransaction.BuySupply + (exchangeTransaction.Units - offerTransaction.BuyLimit), 0);
                        offerTransaction.BuySupply += exchangeTransaction.Units - excess;

                        exchangeTransaction.NqtAmount = excess * offerTransaction.SellRateNqt;
                    }
                    // I am buying currency through ExchangeOffer
                    else
                    {
                        offerTransaction.BuyLimit -= exchangeTransaction.Units;
                        offerTransaction.BuySupply -= exchangeTransaction.Units;
                        var excess = Math.Max(offerTransaction.SellSupply + (exchangeTransaction.Units - offerTransaction.SellLimit), 0);
                        offerTransaction.SellSupply += exchangeTransaction.Units - excess;

                        exchangeTransaction.NqtAmount = 0;
                    }

                    updatedTransactions.Add(offerTransaction);
                }
                else
                {
                    var existingExchangeTransaction = allTransactions.Single(t => t.NxtId == exchangeTransaction.NxtId);
                    existingExchangeTransaction.NqtAmount += exchangeTransaction.NqtAmount;
                    exchangeTransaction.NqtAmount = 0;
                    updatedTransactions.Add(existingExchangeTransaction);
                }
            }

            var newExchangeTransactions = exchangeTransactions
                .Where(t => t.NqtAmount != 0)
                .Except(allTransactions)
                .ToList();
            newTransactions.AddRange(newExchangeTransactions);

            if (exchangeTransactions.Any())
            {
                await _walletRepository.UpdateLastCurrencyExchange(exchangeTransactions.Max(t => t.Timestamp).AddSeconds(1));
            }
        }

        private async Task<List<MsCurrencyExchangeTransaction>> GetNewExchangeTransactions()
        {
            return (await _nxtServer.GetExchanges(_walletRepository.LastCurrencyExchange))
                .OrderBy(t => t.Height)
                .ToList();
        }

        public async Task ExpireExchangeOffers(List<Transaction> allTransactions, List<Transaction> newTransactions,
            HashSet<Transaction> updatedTransactions, int currentHeight)
        {
            var newExchangeOffers = GetNewExchangeOffers(newTransactions);

            foreach (var newExchangeOffer in newExchangeOffers)
            {
                var previousExchangeOffer = GetTransactionAtHeight(allTransactions, newExchangeOffer.Height - 1, newExchangeOffer.CurrencyId);
                if (previousExchangeOffer != null)
                {
                    var expiredTime = newExchangeOffer.Timestamp.AddSeconds(-1);
                    ExpireOffer(previousExchangeOffer, newTransactions, allTransactions, newExchangeOffer.Height, expiredTime);
                    updatedTransactions.Add(previousExchangeOffer);
                }
            }
            var exchangeOffers = GetAllOffersToExpire(allTransactions, currentHeight);

            foreach (var exchangeOffer in exchangeOffers)
            {
                var block = await _nxtServer.GetBlockAsync(exchangeOffer.ExpirationHeight);
                ExpireOffer(exchangeOffer, newTransactions, allTransactions, exchangeOffer.ExpirationHeight, block.Timestamp);
                updatedTransactions.Add(exchangeOffer);
            }
        }

        private static List<MsPublishExchangeOfferTransaction> GetAllOffersToExpire(List<Transaction> allTransactions, int currentHeight)
        {
            return allTransactions.Where(t => t.TransactionType == TransactionType.PublishExchangeOffer)
                .Cast<MsPublishExchangeOfferTransaction>()
                .Where(t => !t.IsExpired && t.ExpirationHeight < currentHeight)
                .ToList();
        }

        private static List<MsPublishExchangeOfferTransaction> GetNewExchangeOffers(List<Transaction> newTransactions)
        {
            return newTransactions.Where(t => t.TransactionType == TransactionType.PublishExchangeOffer)
                .Cast<MsPublishExchangeOfferTransaction>()
                .OrderBy(t => t.Height)
                .ToList();
        }

        private void ExpireOffer(MsPublishExchangeOfferTransaction exchangeOffer, List<Transaction> newTransactions, 
            List<Transaction> allTransactions, int expireHeight, DateTime expireTimestamp)
        {
            if (exchangeOffer.BuySupply > 0)
            {
                var expireTransaction = new MsExchangeOfferExpiredTransaction
                {
                    AccountFrom = Transaction.GeneratedFromAddress,
                    AccountTo = _walletRepository.NxtAccount.AccountRs,
                    CurrencyId = exchangeOffer.CurrencyId,
                    OfferId = (long)exchangeOffer.NxtId.Value,
                    Height = expireHeight,
                    IsConfirmed = true,
                    Message = "[Expire Exchange Offer]",
                    NqtAmount = exchangeOffer.BuySupply * exchangeOffer.BuyRateNqt,
                    NqtFee = 0,
                    NxtId = null,
                    Timestamp = expireTimestamp,
                    TransactionType = TransactionType.CurrencyOfferExpired,
                    UserIsTransactionRecipient = true,
                    UserIsTransactionSender = false
                };
                newTransactions.Add(expireTransaction);
                allTransactions.Add(expireTransaction);
            }

            exchangeOffer.IsExpired = true;
        }

        private MsPublishExchangeOfferTransaction GetTransactionAtHeight(IList<Transaction> allTransactions, int height, long currencyId)
        {
            var transaction = allTransactions.Where(t => t.TransactionType == TransactionType.PublishExchangeOffer)
                .Cast<MsPublishExchangeOfferTransaction>()
                .Where(t => t.CurrencyId == currencyId && t.Height < height && t.ExpirationHeight > height)
                .OrderBy(t => t.Height)
                .FirstOrDefault();

            return transaction;
        }
    }
}
