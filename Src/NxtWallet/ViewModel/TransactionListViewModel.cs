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
        private readonly IBalanceCalculator _balanceCalculator;
        private ObservableCollection<ViewModelTransaction> _transactions;

        public ObservableCollection<ViewModelTransaction> Transactions
        {
            get { return _transactions; }
            set { Set(ref _transactions, value); }
        }

        public TransactionListViewModel(IWalletRepository walletRepository, INxtServer nxtServer, IBalanceCalculator balanceCalculator)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;
            _balanceCalculator = balanceCalculator;
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
                InsertTransactions(newTransactions);
                var updatedTransactions = _balanceCalculator.Calculate(newTransactions, Transactions);
                await _walletRepository.SaveTransactionsAsync(newTransactions.Select(t => t.Transaction));
                await _walletRepository.UpdateTransactionsAsync(updatedTransactions);
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
