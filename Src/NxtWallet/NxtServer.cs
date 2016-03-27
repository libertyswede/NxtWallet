using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtLib;
using NxtLib.Accounts;
using NxtLib.Local;
using NxtLib.Transactions;
using NxtWallet.Model;
using Transaction = NxtWallet.Model.Transaction;

namespace NxtWallet
{
    public interface INxtServer
    {
        event PropertyChangedEventHandler PropertyChanged;

        bool IsOnline { get; }

        Task<Result<string>> GetBalanceAsync();
        Task<IEnumerable<ITransaction>> GetTransactionsAsync(DateTime lastTimestamp);
        Task<IEnumerable<ITransaction>> GetTransactionsAsync();
        Task<ITransaction> SendMoneyAsync(Account recipient, Amount amount, string message);
    }

    public class NxtServer : ObservableObject, INxtServer
    {
        private readonly IWalletRepository _walletRepository;
        private bool _isOnline;
        private readonly IServiceFactory _serviceFactory;

        public bool IsOnline
        {
            get { return _isOnline; }
            set { Set(ref _isOnline, value); }
        }

        public NxtServer(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
            IsOnline = false;
            _serviceFactory = new ServiceFactory(_walletRepository.NxtServer);
        }

        public async Task<Result<string>> GetBalanceAsync()
        {
            try
            {
                var accountService = _serviceFactory.CreateAccountService();
                var balanceResult = await accountService.GetBalance(_walletRepository.NxtAccount);
                IsOnline = true;
                return new Result<string>(balanceResult.Balance.Nxt.ToFormattedString());
            }
            catch (HttpRequestException)
            {
                IsOnline = false;
            }
            catch (NxtException e)
            {
                if (!e.Message.Equals("Unknown account"))
                {
                    throw;
                }
            }
            return new Result<string>(string.Empty, false);
        }

        //TODO: Phased transactions?
        //TODO: Make multiple calls to get ALL transactions
        public async Task<IEnumerable<ITransaction>> GetTransactionsAsync(DateTime lastTimestamp)
        {
            var transactionList = new List<Transaction>();
            try
            {
                var transactionService = _serviceFactory.CreateTransactionService();
                var transactionsReply = await transactionService.GetBlockchainTransactions(
                    _walletRepository.NxtAccount, lastTimestamp, TransactionSubType.PaymentOrdinaryPayment);
                transactionList.AddRange(transactionsReply.Transactions.Select(serverTransaction => new Transaction(serverTransaction)));
                IsOnline = true;
            }
            catch (HttpRequestException)
            {
                IsOnline = false;
            }
            return transactionList.OrderByDescending(t => t.Timestamp);
        }

        public Task<IEnumerable<ITransaction>> GetTransactionsAsync()
        {
            return GetTransactionsAsync(new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc));
        }

        public async Task<ITransaction> SendMoneyAsync(Account recipient, Amount amount, string message)
        {
            var accountService = _serviceFactory.CreateAccountService();
            var transactionService = _serviceFactory.CreateTransactionService();
            var localTransactionService = new LocalTransactionService();

            var sendMoneyReply = await CreateUnsignedSendMoneyReply(recipient, amount, message, accountService);
            var signedTransaction = localTransactionService.SignTransaction(sendMoneyReply, _walletRepository.SecretPhrase);
            var broadcastReply = await transactionService.BroadcastTransaction(new TransactionParameter(signedTransaction.ToString()));

            IsOnline = true;
            var transaction = new Transaction(sendMoneyReply.Transaction, (long)broadcastReply.TransactionId);
            return transaction;
        }

        private async Task<TransactionCreatedReply> CreateUnsignedSendMoneyReply(Account recipient, Amount amount, string message,
            IAccountService accountService)
        {
            var createTransactionByPublicKey = new CreateTransactionByPublicKey(1440, Amount.Zero, _walletRepository.NxtAccount.PublicKey);
            if (!string.IsNullOrEmpty(message))
            {
                createTransactionByPublicKey.Message = new CreateTransactionParameters.UnencryptedMessage(message);
            }
            var sendMoneyReply = await accountService.SendMoney(createTransactionByPublicKey, recipient, amount);
            return sendMoneyReply;
        }
    }

    public class FakeNxtServer : ObservableObject, INxtServer
    {
        private bool _isOnline = true;

        public bool IsOnline
        {
            get { return _isOnline; }
            set { Set(ref _isOnline, value); }
        }

        public Task<Result<string>> GetBalanceAsync()
        {
            return Task.FromResult(new Result<string>("11.00"));
        }

        public Task<IEnumerable<ITransaction>> GetTransactionsAsync(DateTime lastTimestamp)
        {
            return Task.FromResult(new List<ITransaction>().AsEnumerable());
        }

        public Task<IEnumerable<ITransaction>> GetTransactionsAsync()
        {
            return GetTransactionsAsync(DateTime.UtcNow);
        }

        public Task<ITransaction> SendMoneyAsync(Account recipient, Amount amount, string message)
        {
            return Task.FromResult((ITransaction)new Transaction());
        }
    }
}
