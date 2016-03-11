using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NxtWallet.Model;

namespace NxtWallet.ViewModel
{
    public class OverviewViewModel : BindableBase
    {
        public string NxtAddress { get; set; }

        private string _balance = "0.0";
        public string Balance
        {
            get { return _balance; }
            set { SetProperty(ref _balance, value); }
        }

        private ObservableCollection<Transaction> _transactions;
        public ObservableCollection<Transaction> Transactions
        {
            get { return _transactions; }
            set { SetProperty(ref _transactions, value); }
        }


        private readonly NxtServer _nxtServer = new NxtServer();

        public OverviewViewModel()
        {
            NxtAddress = WalletSettings.NxtAccount.AccountRs;
            _nxtServer.PropertyChanged += NxtServer_PropertyChanged;
        }

        public async Task Loading()
        {
            Balance = await _nxtServer.GetBalance();
            Transactions = new ObservableCollection<Transaction>(await _nxtServer.GetTransactions());
        }

        private void NxtServer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(NxtServer.OnlineStatus)))
            {
                
            }
        }
    }
}