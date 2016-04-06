using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtWallet.Model;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.ViewModel
{
    public class TransactionListViewModel : ViewModelBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private ObservableCollection<Transaction> _transactions;

        public ObservableCollection<Transaction> Transactions
        {
            get { return _transactions; }
            set { Set(ref _transactions, value); }
        }

        public TransactionListViewModel(ITransactionRepository transactionRepository, IBackgroundRunner backgroundRunner)
        {
            backgroundRunner.TransactionAdded += (sender, transaction) =>
            {
                InsertTransaction(transaction);
            };
            backgroundRunner.TransactionBalanceUpdated += (sender, transaction) =>
            {
                var existingTransaction = Transactions.Single(t => t.NxtId == transaction.NxtId);
                existingTransaction.NqtBalance = transaction.NqtBalance;
            };
            backgroundRunner.TransactionConfirmationUpdated += (sender, transaction) =>
            {
                var existingTransaction = Transactions.Single(t => t.NxtId == transaction.NxtId);
                existingTransaction.IsConfirmed = transaction.IsConfirmed;
            };

            _transactionRepository = transactionRepository;
            Transactions = new ObservableCollection<Transaction>();
        }

        public void LoadTransactionsFromRepository()
        {
            var transactions = Task.Run(async () => await _transactionRepository.GetAllTransactionsAsync()).Result;
            InsertTransactions(transactions);
        }

        private void InsertTransactions(IEnumerable<Transaction> transactions)
        {
            foreach (var transaction in transactions.Except(Transactions))
            {
                InsertTransaction(transaction);
            }
        }

        private void InsertTransaction(Transaction transaction)
        {
            if (!Transactions.Any())
            {
                Transactions.Add(transaction);
            }
            else
            {
                var index = GetPreviousTransactionIndex(transaction);
                if (index.HasValue)
                {
                    Transactions.Insert(index.Value, transaction);
                }
                else
                {
                    Transactions.Add(transaction);
                }
            }
        }

        private Transaction GetPreviousTransaction(Transaction transaction)
        {
            return Transactions.FirstOrDefault(t => t.Timestamp.CompareTo(transaction.Timestamp) < 0);
        }

        private int? GetPreviousTransactionIndex(Transaction transaction)
        {
            var previousTransaction = GetPreviousTransaction(transaction);
            return previousTransaction == null ? null : (int?)Transactions.IndexOf(previousTransaction);
        }
    }
}
