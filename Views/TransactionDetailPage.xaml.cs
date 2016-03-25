using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class TransactionDetailPage
    {
        private TransactionDetailViewModel ViewModel => (TransactionDetailViewModel) DataContext;

        public TransactionDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Transaction = (ViewModelTransaction) e.Parameter;
        }
    }
}
