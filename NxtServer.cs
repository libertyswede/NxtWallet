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
using Transaction = NxtWallet.Model.Transaction;

namespace NxtWallet
{
    public interface INxtServer
    {
        event PropertyChangedEventHandler PropertyChanged;

        bool IsOnline { get; }

        Task<string> GetBalanceAsync();
        Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime lastTimestamp);
        Task<Transaction> SendMoneyAsync(Account recipient, Amount amount, string message);
    }

    public class NxtServer : ViewModelBase, INxtServer
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

        public async Task<string> GetBalanceAsync()
        {
            try
            {
                var accountService = _serviceFactory.CreateAccountService();
                var balanceResult = await accountService.GetBalance(_walletRepository.NxtAccount);
                IsOnline = true;
                return balanceResult.Balance.ToFormattedString();
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
            return "0.0";
        }

        //TODO: Phased transactions?
        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime lastTimestamp)
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

        public async Task<Transaction> SendMoneyAsync(Account recipient, Amount amount, string message)
        {
            var accountService = _serviceFactory.CreateAccountService();
            var transactionService = _serviceFactory.CreateTransactionService();
            var localTransactionService = new LocalTransactionService();

            var sendMoneyReply = await CreateUnsignedSendMoneyReply(recipient, amount, message, accountService);
            var signedTransaction = localTransactionService.SignTransaction(sendMoneyReply, _walletRepository.SecretPhrase);
            await transactionService.BroadcastTransaction(new TransactionParameter(signedTransaction.ToString()));

            IsOnline = true;
            var transaction = new Transaction(sendMoneyReply.Transaction);
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

    public class FakeNxtServer : INxtServer
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsOnline { get; set; }
        public Task<string> GetBalanceAsync()
        {
            return Task.FromResult("0.0");
        }

        public Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime lastTimestamp)
        {
            return Task.FromResult(new List<Transaction>().AsEnumerable());
        }

        public Task<Transaction> SendMoneyAsync(Account recipient, Amount amount, string message)
        {
            return Task.FromResult(new Transaction());
        }
    }
}
