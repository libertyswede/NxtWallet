using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;
using NxtWallet.Core.ViewModel.Model;

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
            ViewModel.Transaction = (Transaction) e.Parameter;
        }
    }
}
