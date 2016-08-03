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
        Task<List<LedgerEntry>> GetAccountLedgerEntriesAsync();
        Task<List<LedgerEntry>> GetUnconfirmedAccountLedgerEntriesAsync();
        Task<AccountReply> GetAccountAsync(string recipient);
        Task<LedgerEntry> SendMoneyAsync(Account recipient, Amount amount,
            CreateTransactionParameters.UnencryptedMessage plainMessage,
            CreateTransactionParameters.AlreadyEncryptedMessage encryptedMessage,
            CreateTransactionParameters.AlreadyEncryptedMessageToSelf noteToSelfMessage);

        void UpdateNxtServer(string newServerAddress);
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
            catch (HttpRequestException)
            {
                IsOnline = false;
                throw;
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
                var blockReply = await blockService.GetBlock(BlockLocator.ByBlockId(blockId));
                IsOnline = true;
                return blockReply;
            }
            catch (HttpRequestException)
            {
                IsOnline = false;
                throw;
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
            catch (HttpRequestException)
            {
                IsOnline = false;
                throw;
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
            catch (HttpRequestException)
            {
                IsOnline = false;
                throw;
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

        public async Task<List<LedgerEntry>> GetUnconfirmedAccountLedgerEntriesAsync()
        {
            try
            {
                var transactionService = _serviceFactory.CreateTransactionService();
                var unconfirmedTransactions = await transactionService.GetUnconfirmedTransactions(new List<Account> { _walletRepository.NxtAccount });
                var filteredTransactions = unconfirmedTransactions.UnconfirmedTransactions
                    .Where(t => t.SubType == TransactionSubType.PaymentOrdinaryPayment)
                    .ToList();

                var ledgerEntries = _mapper.Map<List<LedgerEntry>>(filteredTransactions);

                await PostProcessEntries(ledgerEntries);
                foreach (var ledgerEntry in ledgerEntries)
                {
                    if (ledgerEntry.UserIsSender)
                    {
                        ledgerEntry.NqtFee *= -1;
                        ledgerEntry.NqtAmount *= -1;
                    }
                }

                IsOnline = true;
                return ledgerEntries;
            }
            catch (HttpRequestException)
            {
                IsOnline = false;
                throw;
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
                lastTimestamp = AdjustIfGenesisBlock(lastTimestamp);
                var accountService = _serviceFactory.CreateAccountService();

                var firstIndex = 0;
                const int count = 100;
                bool hasMore = true;

                var accountLedgerEntries = new List<AccountLedgerEntry>();

                while (hasMore)
                {
                    var accountLedger = await accountService.GetAccountLedger(_walletRepository.NxtAccount,
                        firstIndex, firstIndex + count - 1, holdingType: "UNCONFIRMED_NXT_BALANCE", includeTransactions: true,
                        requireBlock: _walletRepository.LastLedgerEntryBlockId);

                    var newAccountLedgerEntries = accountLedger.Entries.Where(e => e.Timestamp > lastTimestamp).ToList();

                    accountLedgerEntries.AddRange(newAccountLedgerEntries);
                    firstIndex += count;
                    hasMore = accountLedger.Entries.Any() && accountLedger.Entries.Count == newAccountLedgerEntries.Count;
                }

                accountLedgerEntries = GroupLedgerEntries(accountLedgerEntries);

                ledgerEntries.AddRange(_mapper.Map<List<LedgerEntry>>(accountLedgerEntries));
                await PostProcessEntries(ledgerEntries);
                IsOnline = true;
            }
            catch (HttpRequestException)
            {
                IsOnline = false;
                throw;
            }
            catch (JsonReaderException e)
            {
                IsOnline = false;
                throw new Exception("Error when parsing response", e);
            }
            return ledgerEntries;
        }

        private async Task PostProcessEntries(List<LedgerEntry> ledgerEntries)
        {
            UpdateIsMyAddress(ledgerEntries);
            await UpdateEncryptedMessage(ledgerEntries);
        }

        private async Task UpdateEncryptedMessage(List<LedgerEntry> ledgerEntries)
        {
            foreach (var ledgerEntry in ledgerEntries.Where(e => e?.Transaction?.EncryptedMessage != null && e.UserIsSender))
            {
                var messageService = new LocalMessageService();
                var encryptedMessage = ledgerEntry.Transaction.EncryptedMessage;
                var recipienctPublicKey = await GetAccountPublicKeyAsync(ledgerEntry.AccountTo);

                var sharedKey = messageService.GetSharedKey(recipienctPublicKey, encryptedMessage.Nonce, _walletRepository.SecretPhrase);
                var unencrypted = messageService.DecryptText(encryptedMessage.Data, encryptedMessage.Nonce, encryptedMessage.IsCompressed, sharedKey);
                ledgerEntry.EncryptedMessage = unencrypted;
            }
        }

        private static DateTime AdjustIfGenesisBlock(DateTime lastTimestamp)
        {
            if (lastTimestamp == Constants.EpochBeginning)
            {
                lastTimestamp = lastTimestamp.AddSeconds(-1);
            }

            return lastTimestamp;
        }

        private List<AccountLedgerEntry> GroupLedgerEntries(List<AccountLedgerEntry> ledgerEntries)
        {
            var feeEntries = ledgerEntries.Where(e => e.EventType == "TRANSACTION_FEE")
                    .OrderBy(s => s.LedgerId)
                    .ToList();
            var others = ledgerEntries.Except(feeEntries).ToList();

            foreach (var other in others.Where(o => o.Transaction != null))
            {
                other.Transaction.Fee = Amount.Zero;
            }

            var grouped = new List<AccountLedgerEntry>();

            foreach (var feeEntry in feeEntries)
            {
                var firstSibling = others.Where(s => s.IsTransactionEvent &&
                    s.Transaction?.TransactionId == feeEntry.Transaction?.TransactionId &&
                    s.Height == feeEntry.Height &&
                    s.Transaction.SenderRs == myAccountRs)
                    .OrderBy(s => s.LedgerId)
                    .FirstOrDefault();

                if (firstSibling != null)
                {
                    firstSibling.Transaction.Fee = Amount.CreateAmountFromNqt(feeEntry.Change);
                    ledgerEntries.Remove(feeEntry);

                    var affectedEntryBalances = ledgerEntries.Where(e => e.Height == feeEntry.Height && e != feeEntry &&
                                                                         e.LedgerId >= Math.Min(feeEntry.LedgerId, firstSibling.LedgerId) &&
                                                                         e.LedgerId < Math.Max(feeEntry.LedgerId, firstSibling.LedgerId))
                                                                         .ToList();

                    if (feeEntry.LedgerId < firstSibling.LedgerId)
                    {
                        affectedEntryBalances.ForEach(e => e.Balance -= feeEntry.Change);
                    }
                    else
                    {
                        affectedEntryBalances.ForEach(e => e.Balance += feeEntry.Change);
                    }
                }
                else
                {
                    feeEntry.Transaction.Fee = Amount.CreateAmountFromNqt(feeEntry.Change);
                    feeEntry.Change = 0;
                }
            }
            return ledgerEntries;
        }

        public Task<List<LedgerEntry>> GetAccountLedgerEntriesAsync()
        {
            return GetAccountLedgerEntriesAsync(new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc));
        }

        private async Task<string> GetAccountPublicKeyAsync(string recipient)
        {
            try
            {
                var accountService = _serviceFactory.CreateAccountService();
                var accountPublicKeyReply = await accountService.GetAccountPublicKey(recipient);
                IsOnline = true;
                return accountPublicKeyReply.PublicKey;
            }
            catch (HttpRequestException)
            {
                IsOnline = false;
                throw;
            }
            catch (JsonReaderException e)
            {
                IsOnline = true;
                throw new Exception("Error when parsing response", e);
            }
        }

        public async Task<AccountReply> GetAccountAsync(string recipient)
        {
            try
            {
                var accountService = _serviceFactory.CreateAccountService();
                var account = await accountService.GetAccount(recipient);
                IsOnline = true;
                return account;
            }
            catch (HttpRequestException)
            {
                IsOnline = false;
                throw;
            }
            catch (JsonReaderException e)
            {
                IsOnline = true;
                throw new Exception("Error when parsing response", e);
            }
        }

        public async Task<LedgerEntry> SendMoneyAsync(Account recipient, Amount amount, 
            CreateTransactionParameters.UnencryptedMessage plainMessage,
            CreateTransactionParameters.AlreadyEncryptedMessage encryptedMessage,
            CreateTransactionParameters.AlreadyEncryptedMessageToSelf noteToSelfMessage)
        {
            try
            {
                var transactionService = _serviceFactory.CreateTransactionService();
                var localTransactionService = new LocalTransactionService();

                var sendMoneyReply = await CreateUnsignedSendMoneyReply(recipient, amount, plainMessage, encryptedMessage, noteToSelfMessage);
                var signedTransaction = localTransactionService.SignTransaction(sendMoneyReply, _walletRepository.SecretPhrase);
                var broadcastReply = await transactionService.BroadcastTransaction(new TransactionParameter(signedTransaction.ToString()));

                IsOnline = true;

                var ledgerEntry = _mapper.Map<LedgerEntry>(sendMoneyReply.Transaction);
                UpdateIsMyAddress(ledgerEntry);
                ledgerEntry.TransactionId = broadcastReply.TransactionId;
                ledgerEntry.NqtFee *= -1;
                ledgerEntry.NqtAmount *= -1;
                ledgerEntry.IsConfirmed = false;
                return ledgerEntry;
            }
            catch (HttpRequestException)
            {
                IsOnline = false;
                throw;
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

        private async Task<TransactionCreatedReply> CreateUnsignedSendMoneyReply(Account recipient, Amount amount,
            CreateTransactionParameters.UnencryptedMessage plainMessage,
            CreateTransactionParameters.AlreadyEncryptedMessage encryptedMessage,
            CreateTransactionParameters.AlreadyEncryptedMessageToSelf noteToSelfMessage)
        {
            var accountService = _serviceFactory.CreateAccountService();
            var createTransactionByPublicKey = new CreateTransactionByPublicKey(1440, Amount.Zero, _walletRepository.NxtAccountWithPublicKey.PublicKey);

            createTransactionByPublicKey.Message = plainMessage;
            createTransactionByPublicKey.EncryptedMessage = encryptedMessage;
            createTransactionByPublicKey.EncryptedMessageToSelf = noteToSelfMessage;

            var sendMoneyReply = await accountService.SendMoney(createTransactionByPublicKey, recipient, amount);
            return sendMoneyReply;
        }

        private void UpdateIsMyAddress(List<LedgerEntry> ledgerEntries)
        {
            ledgerEntries.ForEach(t => UpdateIsMyAddress(t));
        }

        private void UpdateIsMyAddress(LedgerEntry ledgerEntry)
        {
            ledgerEntry.UserIsRecipient = myAccountRs == ledgerEntry.AccountTo;
            ledgerEntry.UserIsSender = myAccountRs == ledgerEntry.AccountFrom;
        }
    }
}
