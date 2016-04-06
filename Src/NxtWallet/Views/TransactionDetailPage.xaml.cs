using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;
using NxtWallet.ViewModel.Model;

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
            ViewModel.Transaction = (TransactionModel) e.Parameter;
        }
    }
}
