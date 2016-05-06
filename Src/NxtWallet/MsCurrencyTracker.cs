using NxtWallet.Model;
using NxtWallet.ViewModel.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace NxtWallet
{
    public interface IMsCurrencyTracker
    {
        Task CheckMsExchanges(IReadOnlyCollection<Transaction> allTransactions, List<Transaction> newTransactions, 
            ICollection<Transaction> updatedTransactions);
        Task CheckExpiredExchangeOffers(List<Transaction> newTransactions, List<Transaction> allTransactions,
            List<Transaction> updatedTransactions, int currentHeight);
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

        public async Task CheckMsExchanges(IReadOnlyCollection<Transaction> allTransactions, List<Transaction> newTransactions, ICollection<Transaction> updatedTransactions)
        {
            var exchangeTransactions = (await _nxtServer.GetExchanges(_walletRepository.LastCurrencyExchange)).ToList();

            foreach (var exchangeTransaction in exchangeTransactions.OrderBy(t => t.Height))
            {
                var otherTransaction = allTransactions.SingleOrDefault(t => t.NxtId == (ulong)exchangeTransaction.OfferNxtId);
                if (otherTransaction != null)
                {
                    exchangeTransaction.NxtId = (ulong)exchangeTransaction.TransactionNxtId;
                    var offerTransaction = (MsPublishExchangeOfferTransaction)otherTransaction;

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
                    exchangeTransaction.NxtId = (ulong)exchangeTransaction.OfferNxtId;
                }
            }

            var newExchangeTransactions = exchangeTransactions
                .Where(t => t.NqtAmount != 0)
                .Except(allTransactions)
                .ToList();
            newTransactions.AddRange(newExchangeTransactions);

            if (newExchangeTransactions.Any())
            {
                await
                    _walletRepository.UpdateLastCurrencyExchange(
                        newExchangeTransactions.Max(t => t.Timestamp).AddSeconds(1));
            }
        }

        public async Task CheckExpiredExchangeOffers(List<Transaction> newTransactions, List<Transaction> allTransactions, 
            List<Transaction> updatedTransactions, int currentHeight)
        {
            var newExchangeOffers = newTransactions.Where(t => t.TransactionType == TransactionType.PublishExchangeOffer)
                .Cast<MsPublishExchangeOfferTransaction>()
                .ToList();

            foreach (var newExchangeOffer in newExchangeOffers.OrderBy(t => t.Height))
            {
                var previousExchangeOffer = GetTransactionAtHeight(allTransactions, newExchangeOffer.Height - 1, newExchangeOffer.CurrencyId);
                if (previousExchangeOffer != null)
                {
                    ExpireOffer(previousExchangeOffer, newTransactions, allTransactions, newExchangeOffer.Height, newExchangeOffer.Timestamp.AddSeconds(-1));
                    updatedTransactions.Add(previousExchangeOffer);
                }
            }

            var exchangeOffers = allTransactions.Where(t => t.TransactionType == TransactionType.PublishExchangeOffer)
                .Cast<MsPublishExchangeOfferTransaction>()
                .Where(t => !t.IsExpired && t.ExpirationHeight < currentHeight)
                .ToList();

            foreach (var exchangeOffer in exchangeOffers)
            {
                var block = await _nxtServer.GetBlockAsync(exchangeOffer.ExpirationHeight);
                ExpireOffer(exchangeOffer, newTransactions, allTransactions, exchangeOffer.ExpirationHeight, block.Timestamp);
                updatedTransactions.Add(exchangeOffer);
            }
        }

        private void ExpireOffer(MsPublishExchangeOfferTransaction exchangeOffer, List<Transaction> newTransactions, 
            List<Transaction> allTransactions, int expireHeight, DateTime expireTimestamp)
        {
            if (exchangeOffer.BuySupply > 0)
            {
                var expireTransaction = new MsExchangeOfferExpiredTransaction
                {
                    AccountFrom = "Generated",
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

        private void ReplaceOffer(MsPublishExchangeOfferTransaction previousOffer, MsPublishExchangeOfferTransaction offerTransaction)
        {
            

            // Create new offer
            throw new NotImplementedException();
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
