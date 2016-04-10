using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class ReceiveMoneyPage
    {
        private ReceiveMoneyViewModel ViewModel => (ReceiveMoneyViewModel) DataContext;

        public ReceiveMoneyPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.OnNavigatedTo();
        }
    }
}
