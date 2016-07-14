using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;
using NxtWallet.Core.Models;

namespace NxtWallet.Views
{
    public sealed partial class LedgerEntryDetailPage
    {
        private LedgerEntryDetailViewModel ViewModel => (LedgerEntryDetailViewModel) DataContext;

        public LedgerEntryDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.LedgerEntry = (LedgerEntry) e.Parameter;
        }
    }
}
