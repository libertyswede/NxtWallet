using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NxtWallet.ViewModel;

namespace NxtWallet.Controls
{
    public sealed partial class TransactionListControl
    {
        public delegate void SelectedTransactionChangedHandler(object source, SelectionChangedEventArgs e);
        public event SelectedTransactionChangedHandler SelectedTransactionChanged;

        private TransactionListViewModel ViewModel => (TransactionListViewModel) DataContext;

        public TransactionListControl()
        {
            InitializeComponent();
        }

        private async void TransactionListControl_OnLoading(FrameworkElement sender, object args)
        {
            ViewModel.LoadTransactionsFromRepository();
            Bindings.Update();
            await ViewModel.LoadFromNxtServerAsync();
            Bindings.Update();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTransactionChanged?.Invoke(this, e);
        }
    }
}
