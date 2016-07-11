using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NxtWallet.ViewModel;

namespace NxtWallet.Controls
{
    public sealed partial class LedgerEntryListControl
    {
        public delegate void SelectedLedgerEntryChangedHandler(object source, SelectionChangedEventArgs e);
        public event SelectedLedgerEntryChangedHandler SelectedLedgerEntryChanged;

        private LedgerEntryListViewModel ViewModel => (LedgerEntryListViewModel) DataContext;

        public LedgerEntryListControl()
        {
            InitializeComponent();
        }

        private void LedgerEntryListControl_OnLoading(FrameworkElement sender, object args)
        {
            ViewModel.LoadLedgerEntriesFromRepository();
            Bindings.Update();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedLedgerEntryChanged?.Invoke(this, e);
        }
    }
}
