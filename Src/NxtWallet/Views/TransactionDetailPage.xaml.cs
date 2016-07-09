using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;
using NxtWallet.Core.Models;

namespace NxtWallet.Views
{
    public sealed partial class TransactionDetailPage
    {
        private LedgerEntryDetailViewModel ViewModel => (LedgerEntryDetailViewModel) DataContext;

        public TransactionDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.LedgerEntry = (LedgerEntry) e.Parameter;
        }
    }
}
