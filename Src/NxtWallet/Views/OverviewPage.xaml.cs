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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.LoadFromRepository();
            ViewModel.SelectedLedgerEntry = e.Parameter as LedgerEntry;
        }

        private void OnSelectedLedgerEntryChanged(object source, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            UpdateDetailsVisualState();
        }

        private void VisualStateGroupWindowSize_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"VisualStateGroupWindowSize to {e.NewState.Name} {new System.Random().Next(1, 100)}");
            UpdateDetailsVisualState();
        }

        private void UpdateDetailsVisualState()
        {
            if (ViewModel.SelectedLedgerEntry != null)
            {
                if (VisualStateGroupWindowSize.CurrentState == VisualStateMin720 && DetailsColumn.Width.Value != 260)
                {
                    DetailsColumn.Width = new GridLength(260); //TODO I'd reeeaaally like to have this in markup..
                }
                else if ((VisualStateGroupWindowSize.CurrentState == VisualStateMin540 || VisualStateGroupWindowSize.CurrentState == VisualStateMin0))
                {
                    Frame.Navigate(typeof(LedgerEntryDetailPage), ViewModel.SelectedLedgerEntry);
                    //ViewModel.SelectedLedgerEntry = null;
                }
            }
        }

        private void VisualStateGroupLedgerListSize_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"VisualStateGroupLedgerListSize to {e.NewState.Name} {new System.Random().Next(1, 100)}");
        }
    }
}
