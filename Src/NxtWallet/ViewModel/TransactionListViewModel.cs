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

        public void LoadTransactionsFromRepository()
        {
            var transactions = Task.Run(async () => await _walletRepository.GetAllTransactionsAsync()).Result;
            InsertTransactions(transactions.Select(t => new ViewModelTransaction(t, _walletRepository.NxtAccount.AccountRs)));
        }

        private void InsertTransactions(IEnumerable<ViewModelTransaction> transactions)
        {
            foreach (var transaction in transactions.Except(Transactions))
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
        }

        public async Task LoadFromNxtServerAsync()
        {
            var newTransactions = (await _nxtServer.GetTransactionsAsync())
                .Select(t => new ViewModelTransaction(t, _walletRepository.NxtAccount.AccountRs))
                .Except(Transactions)
                .ToList();

            if (newTransactions.Any())
            {
                var firstNewTransaction = newTransactions.OrderBy(t => t.Timestamp).First();

                InsertTransactions(newTransactions);
                UpdateTransactionBalance(firstNewTransaction);
                var updatedTransactions = UpdateSubsequentTransactionBalances(firstNewTransaction);
                updatedTransactions = updatedTransactions.Except(newTransactions.Select(t => t.Transaction));
                await _walletRepository.SaveTransactionsAsync(newTransactions.Select(t => t.Transaction));
                await _walletRepository.UpdateTransactionsAsync(updatedTransactions);
            }
        }

        private IEnumerable<ITransaction> UpdateSubsequentTransactionBalances(ViewModelTransaction viewModelTransaction)
        {
            var updatedTransactions = new HashSet<ITransaction>();
            
            foreach (var subsequentTransaction in GetSubsequentTransactions(viewModelTransaction))
            {
                UpdateTransactionBalance(subsequentTransaction);
                updatedTransactions.Add(subsequentTransaction.Transaction);
            }

            return updatedTransactions;
        }

        private void UpdateTransactionBalance(ViewModelTransaction viewModelTransaction)
        {
            var previousBalance = GetPreviousTransaction(viewModelTransaction)?.Transaction?.NqtBalance ?? 0;
            var transaction = viewModelTransaction.Transaction;

            if (transaction.IsReceived(_walletRepository.NxtAccount.AccountRs))
            {
                viewModelTransaction.SetBalance(previousBalance + transaction.NqtAmount);
            }
            else
            {
                viewModelTransaction.SetBalance(previousBalance - (transaction.NqtAmount + transaction.NqtFeeAmount));
            }
        }

        private IEnumerable<ViewModelTransaction> GetSubsequentTransactions(ViewModelTransaction transaction)
        {
            return Transactions.Where(t => t.Timestamp.CompareTo(transaction.Timestamp) > 0).ToList();
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
