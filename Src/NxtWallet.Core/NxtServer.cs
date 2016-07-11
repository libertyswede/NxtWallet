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
using NxtLib.Blocks;
using NxtLib.Local;
using NxtLib.ServerInfo;
using NxtLib.Transactions;
using NxtWallet.Core.Models;
using NxtWallet.Core.Repositories;

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
        Task<List<LedgerEntry>> GetAccountLedgerEntriesAsync(DateTime lastTimestamp);
        Task<List<LedgerEntry>> GetAccountLedgerEntriesAsync(string account, TransactionSubType transactionSubType);
        Task<List<LedgerEntry>> GetAccountLedgerEntriesAsync();
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
        private string myAccountRs => _walletRepository.NxtAccount.AccountRs;

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

        public async Task<List<LedgerEntry>> GetAccountLedgerEntriesAsync(DateTime lastTimestamp)
        {
            var ledgerEntries = new List<LedgerEntry>();
            try
            {
                var accountService = _serviceFactory.CreateAccountService();
                var accountLedger = await accountService.GetAccountLedger(_walletRepository.NxtAccount,
                    holdingType: "UNCONFIRMED_NXT_BALANCE", includeTransactions: true);
                var entries = GroupLedgerEntries(accountLedger.Entries);

                ledgerEntries.AddRange(_mapper.Map<List<LedgerEntry>>(entries));
                UpdateIsMyAddress(ledgerEntries);
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
            return ledgerEntries.OrderByDescending(t => t.Timestamp).ToList();
        }

        private List<AccountLedgerEntry> GroupLedgerEntries(List<AccountLedgerEntry> ledgerEntries)
        {
            var feeEntries = ledgerEntries.Where(e => e.EventType == "TRANSACTION_FEE").ToList();
            var others = ledgerEntries.Except(feeEntries).ToList();
            var grouped = new List<AccountLedgerEntry>();

            foreach (var feeEntry in feeEntries)
            {
                var other = others.SingleOrDefault(e => e.IsTransactionEvent && 
                    e.Transaction?.TransactionId == feeEntry.Transaction?.TransactionId && 
                    e.Timestamp.Equals(feeEntry.Timestamp));

                if (other != null && other.Transaction.SenderRs == myAccountRs)
                {
                    other.Transaction.Fee = Amount.CreateAmountFromNqt(Math.Abs(feeEntry.Change));
                }
                else
                {
                    grouped.Add(feeEntry);
                }
            }
            return grouped.Union(others).ToList();
        }

        public async Task<List<LedgerEntry>> GetAccountLedgerEntriesAsync(string account, TransactionSubType transactionSubType)
        {
            var ledgerEntries = new List<LedgerEntry>();
            try
            {
                var transactionService = _serviceFactory.CreateTransactionService();
                var transactions = await transactionService.GetBlockchainTransactions(account, transactionType: transactionSubType);
                ledgerEntries.AddRange(_mapper.Map<List<LedgerEntry>>(transactions.Transactions));
                UpdateIsMyAddress(ledgerEntries);
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
            return ledgerEntries.OrderByDescending(t => t.Timestamp).ToList();
        }

        public Task<List<LedgerEntry>> GetAccountLedgerEntriesAsync()
        {
            return GetAccountLedgerEntriesAsync(new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc));
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
                ledgerEntry.TransactionId = broadcastReply.TransactionId;
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

        private void UpdateIsMyAddress(List<LedgerEntry> ledgerEntries)
        {
            ledgerEntries.ForEach(t => UpdateIsMyAddress(t));
        }

        private void UpdateIsMyAddress(LedgerEntry ledgerEntry)
        {
            ledgerEntry.UserIsTransactionRecipient = myAccountRs == ledgerEntry.AccountTo;
            ledgerEntry.UserIsTransactionSender = myAccountRs == ledgerEntry.AccountFrom;
        }
    }
}
