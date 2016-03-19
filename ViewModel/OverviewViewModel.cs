using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.UI.Xaml;
using NxtWallet.Model;

namespace NxtWallet.ViewModel
{
    public class OverviewViewModel : BindableBase
    {
        private string _balance = "0.0";
        private readonly NxtServer _nxtServer;
        private ObservableCollection<Transaction> _transactions;

        public string NxtAddress { get; set; }

        public string Balance
        {
            get { return _balance; }
            set { SetProperty(ref _balance, value); }
        }

        public ObservableCollection<Transaction> Transactions
        {
            get { return _transactions; }
            set { SetProperty(ref _transactions, value); }
        }

        public OverviewViewModel(IWalletRepository walletRepository)
        {
            _nxtServer = new NxtServer(walletRepository);
            NxtAddress = walletRepository.NxtAccount.AccountRs;
            Balance = walletRepository.Balance;
            var transactions = Task.Run(async () => await walletRepository.GetAllTransactionsAsync()).Result;
            Transactions = new ObservableCollection<Transaction>(transactions.OrderByDescending(t => t.Timestamp));
            _nxtServer.PropertyChanged += NxtServer_PropertyChanged;
        }

        public async Task Loading()
        {
            Balance = await _nxtServer.GetBalance();
            var transactions = await _nxtServer.GetTransactions();
            Transactions = new ObservableCollection<Transaction>(transactions.OrderByDescending(t => t.Timestamp));
        }

        private void NxtServer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(NxtServer.OnlineStatus)))
            {
                
            }
        }
    }
}