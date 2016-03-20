using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtLib;
using NxtLib.Local;
using NxtLib.Transactions;

namespace NxtWallet
{
    public interface INxtServer
    {
        event PropertyChangedEventHandler PropertyChanged;

        OnlineStatus OnlineStatus { get; set; }

        Task<string> GetBalanceAsync();
        Task<IEnumerable<Model.Transaction>> GetTransactionsAsync();
        Task<Model.Transaction> SendMoneyAsync(Account recipient, Amount amount, string message);
    }

    public class NxtServer : ViewModelBase, INxtServer
    {
        private readonly IWalletRepository _walletRepository;
        private OnlineStatus _onlineStatus;
        private readonly ServiceFactory _serviceFactory;

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
            var accountService = _serviceFactory.CreateAccountService();
            try
            {
                var balanceResult = await accountService.GetBalance(_walletRepository.NxtAccount);
                await _walletRepository.SaveBalanceAsync(balanceResult.Balance.Nxt.ToString("##.########"));
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
                await _walletRepository.SaveBalanceAsync("0.0");
            }
            return _walletRepository.Balance;
        }

        public async Task<IEnumerable<Model.Transaction>> GetTransactionsAsync()
        {
            var transactionService = _serviceFactory.CreateTransactionService();
            var transactionList = (await _walletRepository.GetAllTransactionsAsync()).ToList();
            try
            {
                var lastTimestamp = transactionList.Any()
                    ? transactionList.Max(t => t.Timestamp)
                    : new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc);

                //TODO: Phased transactions?
                var transactionsReply = await transactionService.GetBlockchainTransactions(
                    _walletRepository.NxtAccount, lastTimestamp, TransactionSubType.PaymentOrdinaryPayment);

                foreach (var serverTransaction in transactionsReply.Transactions.Where(t => transactionList.All(t2 => t2.GetTransactionId() != t.TransactionId)))
                {
                    var nxtTransaction = new Model.Transaction
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        NxtId = (long)serverTransaction.TransactionId.Value,
                        Message = serverTransaction.Message?.MessageText,
                        Timestamp = serverTransaction.Timestamp,

                        NqtAmount = serverTransaction.Recipient == _walletRepository.NxtAccount.AccountId
                            ? serverTransaction.Amount.Nqt
                            : serverTransaction.Amount.Nqt*-1,

                        Account = serverTransaction.Recipient == _walletRepository.NxtAccount.AccountId
                            ? serverTransaction.SenderRs
                            : serverTransaction.RecipientRs
                    };

                    transactionList.Add(nxtTransaction);
                }
                await _walletRepository.SaveTransactionsAsync(transactionList);
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

            var publicKey = new CreateTransactionByPublicKey(1440, Amount.OneNxt, _walletRepository.NxtAccount.PublicKey);
            if (!string.IsNullOrEmpty(message))
            {
                publicKey.Message = new CreateTransactionParameters.UnencryptedMessage(message);
            }
            var sendMoneyReply = await accountService.SendMoney(publicKey, recipient, amount);
            var signedTransaction = localTransactionService.SignTransaction(sendMoneyReply, _walletRepository.SecretPhrase);
            var broadcastReply = await transactionService.BroadcastTransaction(new TransactionParameter(signedTransaction.ToString()));

            var transaction = new Model.Transaction
            {
                NxtId = (long) broadcastReply.TransactionId,
                Account = recipient.AccountRs,
                Message = message,
                NqtAmount = amount.Nqt * -1,
                Timestamp = sendMoneyReply.Transaction.Timestamp
            };
            return transaction;
        }
    }

    public enum OnlineStatus
    {
        Online,
        Offline,
        Synchronizing
    }
}
