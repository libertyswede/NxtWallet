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

namespace NxtWallet
{
    public interface INxtServer
    {
        event PropertyChangedEventHandler PropertyChanged;

        OnlineStatus OnlineStatus { get; set; }

        Task<string> GetBalanceAsync();
        Task<IEnumerable<Model.Transaction>> GetTransactionsAsync(DateTime lastTimestamp);
        Task<Model.Transaction> SendMoneyAsync(Account recipient, Amount amount, string message);
    }

    public class NxtServer : ViewModelBase, INxtServer
    {
        private readonly IWalletRepository _walletRepository;
        private OnlineStatus _onlineStatus;
        private readonly IServiceFactory _serviceFactory;

        public OnlineStatus OnlineStatus
        {
            get { return _onlineStatus; }
            set { Set(ref _onlineStatus, value); }
        }

        public NxtServer(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
            _serviceFactory = new ServiceFactory(_walletRepository.NxtServer);
        }

        public async Task<string> GetBalanceAsync()
        {
            try
            {
                var accountService = _serviceFactory.CreateAccountService();
                var balanceResult = await accountService.GetBalance(_walletRepository.NxtAccount);
                return balanceResult.Balance.ToFormattedString();
            }
            catch (HttpRequestException)
            {
                OnlineStatus = OnlineStatus.Offline;
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
        public async Task<IEnumerable<Model.Transaction>> GetTransactionsAsync(DateTime lastTimestamp)
        {
            var transactionList = new List<Model.Transaction>();
            try
            {
                var transactionService = _serviceFactory.CreateTransactionService();
                var transactionsReply = await transactionService.GetBlockchainTransactions(
                    _walletRepository.NxtAccount, lastTimestamp, TransactionSubType.PaymentOrdinaryPayment);
                transactionList.AddRange(transactionsReply.Transactions.Select(serverTransaction => new Model.Transaction(serverTransaction)));
            }
            catch (HttpRequestException)
            {
                OnlineStatus = OnlineStatus.Offline;
            }
            return transactionList.OrderByDescending(t => t.Timestamp);
        }

        public async Task<Model.Transaction> SendMoneyAsync(Account recipient, Amount amount, string message)
        {
            var accountService = _serviceFactory.CreateAccountService();
            var transactionService = _serviceFactory.CreateTransactionService();
            var localTransactionService = new LocalTransactionService();

            var sendMoneyReply = await CreateUnsignedSendMoneyReply(recipient, amount, message, accountService);
            var signedTransaction = localTransactionService.SignTransaction(sendMoneyReply, _walletRepository.SecretPhrase);
            await transactionService.BroadcastTransaction(new TransactionParameter(signedTransaction.ToString()));

            var transaction = new Model.Transaction(sendMoneyReply.Transaction);
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

    public enum OnlineStatus
    {
        Online,
        Offline,
        Synchronizing
    }
}
