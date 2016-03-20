using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
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
        }

        public void LoadFromRepository()
        {
            Balance = _walletRepository.Balance;
            var transactions = Task.Run(async () => await _walletRepository.GetAllTransactionsAsync()).Result;
            Transactions = new ObservableCollection<Transaction>(transactions);
        }

        public async Task LoadFromNxtServerAsync()
        {
            Balance = await _nxtServer.GetBalanceAsync();
            await _walletRepository.SaveBalanceAsync(Balance);
            var lastTimestamp = Transactions.Any()
                ? Transactions.Max(t => t.Timestamp)
                : new DateTime(2013, 11, 24, 12, 0, 0, DateTimeKind.Utc);
            var transactions = (await _nxtServer.GetTransactionsAsync(lastTimestamp)).ToList();
            await _walletRepository.SaveTransactionsAsync(transactions);
            Transactions = new ObservableCollection<Transaction>(transactions);
        }

        private void NxtServer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(NxtServer.OnlineStatus)))
            {
                
            }
        }
    }
}