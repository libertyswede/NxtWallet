using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NxtLib;
using NxtLib.Local;
using NxtLib.Transactions;

namespace NxtWallet
{
    public class NxtServer : BindableBase
    {
        private OnlineStatus _onlineStatus;
        private readonly ServiceFactory _serviceFactory;

        public OnlineStatus OnlineStatus
        {
            get { return _onlineStatus; }
            set { SetProperty(ref _onlineStatus, value); }
        }

        public NxtServer()
        {
            _serviceFactory = new ServiceFactory(WalletSettings.NxtServer);
        }

        public async Task<string> GetBalance()
        {
            var accountService = _serviceFactory.CreateAccountService();
            try
            {
                var balanceResult = await accountService.GetBalance(WalletSettings.NxtAccount);
                await WalletSettings.SetBalance(balanceResult.Balance.Nxt.ToString("##.########"));
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
                await WalletSettings.SetBalance("0.0");
            }
            return WalletSettings.Balance;
        }

        public async Task<IEnumerable<Model.Transaction>> GetTransactions()
        {
            var transactionService = _serviceFactory.CreateTransactionService();
            var transactionList = (await WalletSettings.GetAllTransactions()).ToList();
            try
            {
                var lastTimestamp = transactionList.Any()
                    ? transactionList.Max(t => t.Timestamp)
                    : new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc);

                //TODO: Phased transactions?
                var transactionsReply = await transactionService.GetBlockchainTransactions(
                    WalletSettings.NxtAccount, lastTimestamp, TransactionSubType.PaymentOrdinaryPayment);

                foreach (var serverTransaction in transactionsReply.Transactions.Where(t => transactionList.All(t2 => t2.GetTransactionId() != t.TransactionId)))
                {
                    var nxtTransaction = new Model.Transaction
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        NxtId = (long)serverTransaction.TransactionId.Value,
                        Message = serverTransaction.Message.MessageText,
                        Timestamp = serverTransaction.Timestamp,

                        NqtAmount = serverTransaction.Recipient == WalletSettings.NxtAccount.AccountId
                            ? serverTransaction.Amount.Nqt
                            : serverTransaction.Amount.Nqt*-1,

                        Account = serverTransaction.Recipient == WalletSettings.NxtAccount.AccountId
                            ? serverTransaction.SenderRs
                            : serverTransaction.RecipientRs
                    };

                    transactionList.Add(nxtTransaction);
                }
            }
            catch (HttpRequestException)
            {
                OnlineStatus = OnlineStatus.Offline;
            }
            return transactionList;
        }

        public async Task SendMoney(Account recipient, Amount amount, string message)
        {
            var accountService = _serviceFactory.CreateAccountService();
            var transactionService = _serviceFactory.CreateTransactionService();
            var localTransactionService = new LocalTransactionService();

            var publicKey = new CreateTransactionByPublicKey(1440, Amount.OneNxt, WalletSettings.NxtAccount.PublicKey);
            if (!string.IsNullOrEmpty(message))
            {
                publicKey.Message = new CreateTransactionParameters.UnencryptedMessage(message);
            }
            var sendMoneyReply = await accountService.SendMoney(publicKey, recipient, amount);
            var signedTransaction = localTransactionService.SignTransaction(sendMoneyReply, WalletSettings.SecretPhrase);
            var broadcastReply = await transactionService.BroadcastTransaction(new TransactionParameter(signedTransaction.ToString()));
        }
    }

    public enum OnlineStatus
    {
        Online,
        Offline,
        Synchronizing
    }
}
