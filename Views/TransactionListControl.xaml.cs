using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Ioc;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class TransactionListControl
    {
        public TransactionListViewModel ViewModel { get; } = new Ioc().TransactionListViewModel;

        public TransactionListControl()
        {
            InitializeComponent();
        }

        private async void TransactionListControl_OnLoading(FrameworkElement sender, object args)
        {
            ViewModel.LoadFromRepository();
            Bindings.Update();
            await ViewModel.LoadFromNxtServerAsync();
            Bindings.Update();
        }
    }
}
