using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using NxtLib;
using NxtLib.Accounts;
using NxtLib.AssetExchange;
using NxtLib.Blocks;
using NxtLib.Local;
using NxtLib.MonetarySystem;
using NxtLib.ServerInfo;
using NxtLib.Transactions;
using NxtWallet.Repositories.Model;
using LedgerEntryType = NxtWallet.Core.Models.LedgerEntryType;
using NxtLib.Shuffling;
using NxtWallet.Core.Models;

namespace NxtWallet.Core
{
    public interface INxtServer
    {
        event PropertyChangedEventHandler PropertyChanged;

        bool IsOnline { get; }

        Task<BlockchainStatus> GetBlockchainStatusAsync();
        Task<Block<ulong>> GetBlockAsync(ulong blockId);
        Task<Block<ulong>> GetBlockAsync(int height);
        Task<long> GetUnconfirmedNqtBalanceAsync();
        Task<IEnumerable<LedgerEntry>> GetTransactionsAsync(DateTime lastTimestamp);
        Task<IEnumerable<LedgerEntry>> GetTransactionsAsync(string account, TransactionSubType transactionSubType);
        Task<IEnumerable<LedgerEntry>> GetTransactionsAsync();
        Task<LedgerEntry> SendMoneyAsync(Account recipient, Amount amount, string message);
        void UpdateNxtServer(string newServerAddress);
        Task<LedgerEntry> GetTransactionAsync(ulong transactionId);
    }

    public class NxtServer : ObservableObject, INxtServer
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IMapper _mapper;
        private bool _isOnline;
        private IServiceFactory _serviceFactory;
        private ulong requireBlock => _walletRepository.LastLedgerEntryBlockId;
        private string accountRs => _walletRepository.NxtAccount.AccountRs;

        public bool IsOnline
        {
            get { return _isOnline; }
            set { Set(ref _isOnline, value); }
        }

        public NxtServer(IWalletRepository walletRepository, IMapper mapper, IServiceFactory serviceFactory)
        {
            _walletRepository = walletRepository;
            _mapper = mapper;
            IsOnline = false;
            _serviceFactory = serviceFactory;
        }

        public async Task<BlockchainStatus> GetBlockchainStatusAsync()
        {
            try
            {
                var serverInfoService = _serviceFactory.CreateServerInfoService();
                var blockchainStatus = await serverInfoService.GetBlockchainStatus();
                IsOnline = true;
                return blockchainStatus;
            }
            catch (HttpRequestException e)
            {
                IsOnline = false;
                throw new Exception("Error when connecting to nxt server", e);
            }
            catch (JsonReaderException e)
            {
                IsOnline = false;
                throw new Exception("Error when parsing response", e);
            }
        }

        public async Task<Block<ulong>> GetBlockAsync(ulong blockId)
        {
            try
            {
                var blockService = _serviceFactory.CreateBlockService();
                var blockReply = await blockService.GetBlock(BlockLocator.ByBlockId(blockId), requireBlock: requireBlock);
                IsOnline = true;
                return blockReply;
            }
            catch (HttpRequestException e)
            {
                IsOnline = false;
                throw new Exception("Error when connecting to nxt server", e);
            }
            catch (JsonReaderException e)
            {
                IsOnline = false;
                throw new Exception("Error when parsing response", e);
            }
        }

        public async Task<Block<ulong>> GetBlockAsync(int height)
        {
            try
            {
                var blockService = _serviceFactory.CreateBlockService();
                var blockReply = await blockService.GetBlock(BlockLocator.ByHeight(height), requireBlock: requireBlock);
                IsOnline = true;
                return blockReply;
            }
            catch (HttpRequestException e)
            {
                IsOnline = false;
                throw new Exception("Error when connecting to nxt server", e);
            }
            catch (JsonReaderException e)
            {
                IsOnline = false;
                throw new Exception("Error when parsing response", e);
            }
        }

        public async Task<long> GetUnconfirmedNqtBalanceAsync()
        {
            try
            {
                var accountService = _serviceFactory.CreateAccountService();
                var balanceResult = await accountService.GetBalance(_walletRepository.NxtAccount, requireBlock: requireBlock);
                IsOnline = true;
                return balanceResult.UnconfirmedBalance.Nqt;
            }
            catch (HttpRequestException e)
            {
                IsOnline = false;
                throw new Exception("Error when connecting to nxt server", e);
            }
            catch (JsonReaderException e)
            {
                IsOnline = false;
                throw new Exception("Error when parsing response", e);
            }
            catch (NxtException e)
            {
                if (!e.Message.Equals("Unknown account"))
                {
                    IsOnline = false;
                    throw new Exception("", e);
                }
            }
            IsOnline = true;
            return 0;
        }

        public async Task<LedgerEntry> GetTransactionAsync(ulong transactionId)
        {
            try
            {
                var transactionService = _serviceFactory.CreateTransactionService();
                var transactionReply = await transactionService.GetTransaction(GetTransactionLocator.ByTransactionId(transactionId),
                    requireBlock: requireBlock);
                IsOnline = true;
                var transaction = _mapper.Map<LedgerEntry>(transactionReply);
                UpdateIsMyAddress(transaction);
                return transaction;
            }
            catch (HttpRequestException e)
            {
                IsOnline = false;
                throw new Exception("Error when connecting to nxt server", e);
            }
            catch (JsonReaderException e)
            {
                IsOnline = false;
                throw new Exception("Error when parsing response", e);
            }
        }

        public async Task<IEnumerable<LedgerEntry>> GetTransactionsAsync(DateTime lastTimestamp)
        {
            var transactionList = new List<LedgerEntry>();
            try
            {
                var transactionService = _serviceFactory.CreateTransactionService();
                var transactions = await transactionService.GetBlockchainTransactions(_walletRepository.NxtAccount, 
                    lastTimestamp, requireBlock: requireBlock);
                var unconfirmed = await transactionService.GetUnconfirmedTransactions(new[] {_walletRepository.NxtAccount},
                    requireBlock);

                transactionList.AddRange(_mapper.Map<List<LedgerEntry>>(transactions.Transactions));
                transactionList.AddRange(_mapper.Map<List<LedgerEntry>>(unconfirmed.UnconfirmedTransactions));
                UpdateIsMyAddress(transactionList);
                IsOnline = true;
            }
            catch (HttpRequestException e)
            {
                IsOnline = false;
                throw new Exception("Error when connecting to nxt server", e);
            }
            catch (JsonReaderException e)
            {
                IsOnline = false;
                throw new Exception("Error when parsing response", e);
            }
            return transactionList.OrderByDescending(t => t.Timestamp);
        }

        public async Task<IEnumerable<LedgerEntry>> GetTransactionsAsync(string account, TransactionSubType transactionSubType)
        {
            var transactionList = new List<LedgerEntry>();
            try
            {
                var transactionService = _serviceFactory.CreateTransactionService();
                var transactions = await transactionService.GetBlockchainTransactions(account, transactionType: transactionSubType);
                transactionList.AddRange(_mapper.Map<List<LedgerEntry>>(transactions.Transactions));
                UpdateIsMyAddress(transactionList);
                IsOnline = true;
            }
            catch (HttpRequestException e)
            {
                IsOnline = false;
                throw new Exception("Error when connecting to nxt server", e);
            }
            catch (JsonReaderException e)
            {
                IsOnline = false;
                throw new Exception("Error when parsing response", e);
            }
            return transactionList.OrderByDescending(t => t.Timestamp);
        }

        public Task<IEnumerable<LedgerEntry>> GetTransactionsAsync()
        {
            return GetTransactionsAsync(new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc));
        }
        
        public async Task<LedgerEntry> SendMoneyAsync(Account recipient, Amount amount, string message)
        {
            try
            {
                var accountService = _serviceFactory.CreateAccountService();
                var transactionService = _serviceFactory.CreateTransactionService();
                var localTransactionService = new LocalTransactionService();

                var sendMoneyReply = await CreateUnsignedSendMoneyReply(recipient, amount, message, accountService);
                var signedTransaction = localTransactionService.SignTransaction(sendMoneyReply, _walletRepository.SecretPhrase);
                var broadcastReply = await transactionService.BroadcastTransaction(new TransactionParameter(signedTransaction.ToString()));

                IsOnline = true;

                var ledgerEntry = _mapper.Map<LedgerEntry>(sendMoneyReply.Transaction);
                UpdateIsMyAddress(ledgerEntry);
                ledgerEntry.NxtId = broadcastReply.TransactionId;
                return ledgerEntry;
            }
            catch (HttpRequestException e)
            {
                IsOnline = false;
                throw new Exception("Error when connecting to nxt server", e);
            }
            catch (JsonReaderException e)
            {
                IsOnline = false;
                throw new Exception("Error when parsing response", e);
            }
        }

        public void UpdateNxtServer(string newServerAddress)
        {
            _serviceFactory = new ServiceFactory(newServerAddress);
        }

        private async Task<TransactionCreatedReply> CreateUnsignedSendMoneyReply(Account recipient, Amount amount, string message,
            IAccountService accountService)
        {
            var createTransactionByPublicKey = new CreateTransactionByPublicKey(1440, Amount.Zero, _walletRepository.NxtAccountWithPublicKey.PublicKey);
            if (!string.IsNullOrEmpty(message))
            {
                createTransactionByPublicKey.Message = new CreateTransactionParameters.UnencryptedMessage(message);
            }
            var sendMoneyReply = await accountService.SendMoney(createTransactionByPublicKey, recipient, amount);
            return sendMoneyReply;
        }

        private void UpdateIsMyAddress<T>(List<T> transactions) where T : LedgerEntry
        {
            transactions.ForEach(t => t.UserIsTransactionRecipient = accountRs == t.AccountTo);
            transactions.ForEach(t => t.UserIsTransactionSender = accountRs == t.AccountFrom);
        }

        private void UpdateIsMyAddress(LedgerEntry transaction)
        {
            transaction.UserIsTransactionRecipient = accountRs == transaction.AccountTo;
            transaction.UserIsTransactionSender = accountRs == transaction.AccountFrom;
        }
    }
}
