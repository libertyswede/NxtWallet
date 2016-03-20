using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class OverviewPage
    {
        public OverviewViewModel ViewModel { get; } = Ioc.OverviewViewModel;

        public OverviewPage()
        {
            InitializeComponent();
        }

        private async void OverviewPage_OnLoading(FrameworkElement sender, object args)
        {
            await ViewModel.LoadFromServerAsync();
            Bindings.Update();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.LoadFromRepository();
            Bindings.Update();
        }
    }
}
