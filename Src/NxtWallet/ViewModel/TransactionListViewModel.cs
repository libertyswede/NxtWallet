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
        private ObservableCollection<ViewModelTransaction> _transactions;

        public ObservableCollection<ViewModelTransaction> Transactions
        {
            get { return _transactions; }
            set { Set(ref _transactions, value); }
        }

        public TransactionListViewModel(IWalletRepository walletRepository, IBackgroundRunner backgroundRunner)
        {
            backgroundRunner.TransactionAdded += (sender, transaction) =>
            {
                InsertTransaction(new ViewModelTransaction(transaction, walletRepository.NxtAccount.AccountRs));
            };
            backgroundRunner.TransactionBalanceUpdated += (sender, transaction) =>
            {
                var existingTransaction = Transactions.Single(t => t.NxtId == transaction.GetTransactionId());
                existingTransaction.SetBalance(transaction.NqtBalance);
            };
            backgroundRunner.TransactionConfirmationUpdated += (sender, transaction) =>
            {
                var existingTransaction = Transactions.Single(t => t.NxtId == transaction.GetTransactionId());
                existingTransaction.IsConfirmed = transaction.IsConfirmed;
            };

            _walletRepository = walletRepository;
            Transactions = new ObservableCollection<ViewModelTransaction>();
        }

        public void LoadTransactionsFromRepository()
        {
            var transactions = Task.Run(async () => await _walletRepository.GetAllTransactionsAsync()).Result;
            InsertTransactions(transactions.Select(t => new ViewModelTransaction(t, _walletRepository.NxtAccount.AccountRs)));
        }

        private void InsertTransactions(IEnumerable<ViewModelTransaction> transactions)
        {
            foreach (var transaction in transactions.Except(Transactions))
            {
                InsertTransaction(transaction);
            }
        }

        private void InsertTransaction(ViewModelTransaction transaction)
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

        private ViewModelTransaction GetPreviousTransaction(ViewModelTransaction transaction)
        {
            return Transactions.FirstOrDefault(t => t.Timestamp.CompareTo(transaction.Timestamp) < 0);
        }

        private int? GetPreviousTransactionIndex(ViewModelTransaction transaction)
        {
            var previousTransaction = GetPreviousTransaction(transaction);
            return previousTransaction == null ? null : (int?)Transactions.IndexOf(previousTransaction);
        }
    }
}
