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
        private ObservableCollection<Transaction> _transactions;

        public string NxtAddress { get; set; }

        public string Balance
        {
            get { return _balance; }
            set { Set(ref _balance, value); }
        }

        public ObservableCollection<Transaction> Transactions
        {
            get { return _transactions; }
            set { Set(ref _transactions, value); }
        }

        public OverviewViewModel(IWalletRepository walletRepository, INxtServer nxtServer)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;
            _nxtServer.PropertyChanged += NxtServer_PropertyChanged;

            Balance = "0.0";
            NxtAddress = walletRepository.NxtAccount.AccountRs;
            Transactions = new ObservableCollection<Transaction>();
        }

        public void LoadFromRepository()
        {
            Balance = _walletRepository.Balance;
            var transactions = Task.Run(async () => await _walletRepository.GetAllTransactionsAsync()).Result;
            AppendTransactions(transactions);
        }

        private void AppendTransactions(IEnumerable<Transaction> transactions)
        {
            foreach (var transaction in transactions.Except(Transactions).OrderByDescending(t => t.Timestamp))
            {
                Transactions.Add(transaction);
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
                .Except(Transactions)
                .ToList();
            
            await _walletRepository.SaveTransactionsAsync(transactions);
            AppendTransactions(transactions);
        }

        private void NxtServer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(NxtServer.OnlineStatus)))
            {
                
            }
        }
    }
}