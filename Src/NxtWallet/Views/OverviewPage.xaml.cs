using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;
using NxtWallet.Core.Models;
using Windows.UI.Xaml;

namespace NxtWallet.Views
{
    public sealed partial class OverviewPage
    {
        private OverviewViewModel ViewModel => (OverviewViewModel) DataContext;

        public OverviewPage()
        {
            InitializeComponent();
            AccountLedgerList.SelectedLedgerEntryChanged += OnSelectedLedgerEntryChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.SelectedLedgerEntry = null;
            ViewModel.LoadFromRepository();
            Bindings.Update();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            AccountLedgerList.SelectedLedgerEntryChanged -= OnSelectedLedgerEntryChanged;
            base.OnNavigatingFrom(e);
        }

        private void OnSelectedLedgerEntryChanged(object source, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var selectedLedgerEntry = selectionChangedEventArgs.AddedItems.SingleOrDefault() as LedgerEntry;
            ViewModel.SelectedLedgerEntry = selectedLedgerEntry;
            UpdateDetailsVisualState();
        }

        private void VisualStateGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateDetailsVisualState();
        }

        private void UpdateDetailsVisualState()
        {
            if (VisualStateGroup.CurrentState == VisualStateMin720 && ViewModel.SelectedLedgerEntry != null && DetailsColumn.Width.Value != 260)
            {
                DetailsColumn.Width = new GridLength(260); //TODO I'd reeeaaally like to have this in markup..
            }
            else if ((VisualStateGroup.CurrentState == VisualStateMin540 || VisualStateGroup.CurrentState == VisualStateMin0) && ViewModel.SelectedLedgerEntry != null)
            {
                Frame.Navigate(typeof(LedgerEntryDetailPage), ViewModel.SelectedLedgerEntry);
                ViewModel.SelectedLedgerEntry = null;
            }
        }
    }
}
