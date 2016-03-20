using Windows.UI.Xaml;
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
    }
}
