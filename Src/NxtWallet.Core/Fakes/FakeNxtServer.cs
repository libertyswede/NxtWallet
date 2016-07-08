using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtLib;
using NxtLib.ServerInfo;
using NxtWallet.Core.Models;

namespace NxtWallet.Core.Fakes
{
    public class FakeNxtServer : ObservableObject, INxtServer
    {
        private bool _isOnline = true;

        public bool IsOnline
        {
            get { return _isOnline; }
            set { Set(ref _isOnline, value); }
        }

        public Task<BlockchainStatus> GetBlockchainStatusAsync()
        {
            return Task.FromResult(new BlockchainStatus());
        }

        public Task<Block<ulong>> GetBlockAsync(ulong blockId)
        {   
            return Task.FromResult(new Block<ulong>());
        }

        public Task<Block<ulong>> GetBlockAsync(int height)
        {
            return Task.FromResult(new Block<ulong>());
        }

        public Task<long> GetUnconfirmedNqtBalanceAsync()
        {
            return Task.FromResult(11 * 100000000L);
        }

        public Task<IEnumerable<LedgerEntry>> GetAccountLedgerEntriesAsync(DateTime lastTimestamp)
        {
            return Task.FromResult(new List<LedgerEntry>().AsEnumerable());
        }

        public Task<IEnumerable<LedgerEntry>> GetAccountLedgerEntriesAsync()
        {
            return GetAccountLedgerEntriesAsync(DateTime.UtcNow);
        }

        public Task<IEnumerable<LedgerEntry>> GetAccountLedgerEntriesAsync(string account, TransactionSubType transactionSubType)
        {
            return Task.FromResult(new List<LedgerEntry>().AsEnumerable());
        }

        public Task<IEnumerable<LedgerEntry>> GetDividendTransactionsAsync(string account, DateTime timestamp)
        {
            return Task.FromResult(new List<LedgerEntry>().AsEnumerable());
        }

        public Task<LedgerEntry> SendMoneyAsync(Account recipient, Amount amount, string message)
        {
            return Task.FromResult(new LedgerEntry());
        }
        public void UpdateNxtServer(string newServerAddress)
        {
        }

        public Task<LedgerEntry> GetTransactionAsync(ulong transactionId)
        {
            return Task.FromResult(new LedgerEntry());
        }
    }
}