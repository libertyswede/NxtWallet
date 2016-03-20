using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NxtWallet.Model;

namespace NxtWallet.ViewModel
{
    public class OverviewViewModel : ViewModelBase
    {
        private string _balance = "0.0";
        private readonly INxtServer _nxtServer;
        private ObservableCollection<Transaction> _transactions;

        public string NxtAddress { get; set; }

        public string Balance
        {
            get { return _balance; }
            set
            {
                _balance = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Transaction> Transactions
        {
            get { return _transactions; }
            set
            {
                _transactions = value;
                RaisePropertyChanged();
            }
        }

        public OverviewViewModel(IWalletRepository walletRepository, INxtServer nxtServer)
        {
            _nxtServer = nxtServer;
            NxtAddress = walletRepository.NxtAccount.AccountRs;
            Balance = walletRepository.Balance;
            var transactions = Task.Run(async () => await walletRepository.GetAllTransactionsAsync()).Result;
            Transactions = new ObservableCollection<Transaction>(transactions);
            _nxtServer.PropertyChanged += NxtServer_PropertyChanged;
        }

        public async Task Loading()
        {
            Balance = await _nxtServer.GetBalanceAsync();
            var transactions = await _nxtServer.GetTransactionsAsync();
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