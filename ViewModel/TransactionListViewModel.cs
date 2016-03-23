using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtWallet.Model;

namespace NxtWallet.ViewModel
{
    public class TransactionListViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly INxtServer _nxtServer;
        private ObservableCollection<ViewModelTransaction> _transactions;

        public ObservableCollection<ViewModelTransaction> Transactions
        {
            get { return _transactions; }
            set { Set(ref _transactions, value); }
        }

        public TransactionListViewModel(IWalletRepository walletRepository, INxtServer nxtServer)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;
            Transactions = new ObservableCollection<ViewModelTransaction>();
        }

        public void LoadFromRepository()
        {
            var transactions = Task.Run(async () => await _walletRepository.GetAllTransactionsAsync()).Result;
            AppendTransactions(transactions);
        }

        private void AppendTransactions(IEnumerable<ITransaction> transactions)
        {
            var viewModelTransactions = transactions.Select(t => new ViewModelTransaction(t, _walletRepository.NxtAccount.AccountRs));
            foreach (var transaction in viewModelTransactions.Except(Transactions).OrderByDescending(t => t.Timestamp))
            {
                Transactions.Add(transaction);
            }
        }

        public async Task LoadFromNxtServerAsync()
        {
            var lastTimestamp = Transactions.Any()
                ? Transactions.Max(t => t.Timestamp)
                : new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc);

            var transactions = (await _nxtServer.GetTransactionsAsync(lastTimestamp))
                .Where(t => Transactions.All(modelTransaction => t.NxtId != (long)modelTransaction.NxtId))
                .ToList();

            UpdateTransactionBalance(transactions);
            await _walletRepository.SaveTransactionsAsync(transactions);
            AppendTransactions(transactions);
        }

        private void UpdateTransactionBalance(IEnumerable<ITransaction> transactions)
        {
            var previousBalance = Transactions.Any() ? Transactions.Last().Transaction.NqtBalance : 0;
            var modelTransactions = Transactions.Select(t => t.Transaction).ToList();

            foreach (var transaction in transactions.Except(modelTransactions).OrderBy(t => t.Timestamp))
            {
                if (transaction.IsReceived(_walletRepository.NxtAccount.AccountRs))
                {
                    transaction.NqtBalance = previousBalance + transaction.NqtAmount;
                }
                else
                {
                    transaction.NqtBalance = previousBalance - (transaction.NqtAmount + transaction.NqtFeeAmount);
                }
                previousBalance = transaction.NqtBalance;
            }
        }
    }
}
