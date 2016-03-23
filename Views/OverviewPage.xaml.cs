using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.ServiceLocation;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class OverviewPage
    {
        public OverviewViewModel ViewModel { get; } = ServiceLocator.Current.GetInstance<OverviewViewModel>();

        public OverviewPage()
        {
            InitializeComponent();
            TransactionList.SelectedTransactionChanged += OnSelectedTransactionChanged;
        }

        private async void OverviewPage_OnLoading(FrameworkElement sender, object args)
        {
            await ViewModel.LoadFromNxtServerAsync();
            Bindings.Update();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.LoadFromRepository();
            Bindings.Update();
        }

        private void OnSelectedTransactionChanged(object source, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (selectionChangedEventArgs.AddedItems.Any())
            {
                Frame.Navigate(typeof(TransactionDetailPage), selectionChangedEventArgs.AddedItems.Single());
            }
        }
    }
}
