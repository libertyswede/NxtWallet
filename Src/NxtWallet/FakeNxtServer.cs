using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtLib;
using NxtLib.Blocks;
using NxtLib.MonetarySystem;
using NxtLib.ServerInfo;
using NxtWallet.ViewModel.Model;
using Transaction = NxtWallet.ViewModel.Model.Transaction;

namespace NxtWallet
{
    public class FakeNxtServer : ObservableObject, INxtServer
    {
        private bool _isOnline = true;

        public bool IsOnline
        {
            get { return _isOnline; }
            set { Set(ref _isOnline, value); }
        }

        public Task<BlockchainStatus> GetCurrentBlockId()
        {
            return Task.FromResult(new BlockchainStatus());
        }

        public Task<GetBlockReply<ulong>> GetBlockAsync(ulong blockId)
        {
            return Task.FromResult(new GetBlockReply<ulong>());
        }

        public Task<GetBlockReply<ulong>> GetBlockAsync(int height)
        {
            return Task.FromResult(new GetBlockReply<ulong>());
        }

        public Task<long> GetBalanceAsync()
        {
            return Task.FromResult(11 * 100000000L);
        }

        public Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime lastTimestamp)
        {
            return Task.FromResult(new List<Transaction>().AsEnumerable());
        }

        public Task<IEnumerable<Transaction>> GetTransactionsAsync()
        {
            return GetTransactionsAsync(DateTime.UtcNow);
        }

        public Task<IEnumerable<Transaction>> GetDividendTransactionsAsync(string account, DateTime timestamp)
        {
            return Task.FromResult(new List<Transaction>().AsEnumerable());
        }

        public Task<Result<Transaction>> SendMoneyAsync(Account recipient, Amount amount, string message)
        {
            return Task.FromResult(new Result<Transaction>());
        }

        public Task<IEnumerable<Transaction>> GetAssetTradesAsync(DateTime timestamp)
        {
            return Task.FromResult(new List<Transaction>().AsEnumerable());
        }

        public Task<Asset> GetAssetAsync(ulong assetId)
        {
            return Task.FromResult(new Asset());
        }

        public void UpdateNxtServer(string newServerAddress)
        {
        }

        public Task<IEnumerable<Transaction>> GetForgingIncomeAsync(DateTime timestamp)
        {
            return Task.FromResult(new List<Transaction>().AsEnumerable());
        }

        public Task<Transaction> GetTransactionAsync(ulong transactionId)
        {
            return Task.FromResult(new Transaction());
        }

        public Task<bool> GetIsPurchaseExpired(ulong purchaseId)
        {
            return Task.FromResult(false);
        }

        public Task<Currency> GetCurrencyAsync(ulong currencyId)
        {
            return Task.FromResult(new Currency());
        }
    }
}