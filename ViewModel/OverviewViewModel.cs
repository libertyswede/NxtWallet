using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtLib.Local;
using NxtWallet.Model;

namespace NxtWallet.ViewModel
{
    public class OverviewViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly INxtServer _nxtServer;

        private string _balance;
        private ObservableCollection<ViewModelTransaction> _transactions;

        public string NxtAddress { get; set; }

        public string Balance
        {
            get { return _balance; }
            set { Set(ref _balance, value); }
        }

        public ObservableCollection<ViewModelTransaction> Transactions
        {
            get { return _transactions; }
            set { Set(ref _transactions, value); }
        }

        public OverviewViewModel(IWalletRepository walletRepository, INxtServer nxtServer)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;

            Balance = "0.0";
            NxtAddress = walletRepository.NxtAccount.AccountRs;
            Transactions = new ObservableCollection<ViewModelTransaction>();
        }

        public void LoadFromRepository()
        {
            Balance = _walletRepository.Balance;
            var transactions = Task.Run(async () => await _walletRepository.GetAllTransactionsAsync()).Result;
            AppendTransactions(transactions);
        }

        private void AppendTransactions(IEnumerable<Transaction> transactions)
        {
            var viewModelTransactions = transactions.Select(t => new ViewModelTransaction(t, _walletRepository.NxtAccount.AccountRs));
            foreach (var transaction in viewModelTransactions.Except(Transactions).OrderByDescending(t => t.Timestamp))
            {
                Transactions.Add(transaction);
            }
        }

        private void UpdateTransactionBalance(IEnumerable<Transaction> transactions)
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

        public async Task LoadFromNxtServerAsync()
        {
            Balance = await _nxtServer.GetBalanceAsync();
            await _walletRepository.SaveBalanceAsync(Balance);
            var lastTimestamp = Transactions.Any()
                ? Transactions.Max(t => t.Timestamp)
                : Constants.EpochBeginning;

            var transactions = (await _nxtServer.GetTransactionsAsync(lastTimestamp))
                .Where(t => Transactions.All(t2 => t.NxtId != t2.NxtId))
                .ToList();
            
            UpdateTransactionBalance(transactions);
            await _walletRepository.SaveTransactionsAsync(transactions);
            AppendTransactions(transactions);
        }
    }
}