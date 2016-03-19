using Windows.UI.Xaml;
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
            await ViewModel.Loading();
            Bindings.Update();
        }
    }
}
