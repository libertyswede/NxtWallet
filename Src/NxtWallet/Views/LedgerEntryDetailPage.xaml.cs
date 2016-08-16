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

        private void VisualStateGroup_CurrentStateChanged(object sender, Windows.UI.Xaml.VisualStateChangedEventArgs e)
        {
            if (e.NewState == VisualStateMin720)
            {
                Frame.Navigate(typeof(OverviewPage), ViewModel.LedgerEntry);
            }
        }
    }
}
