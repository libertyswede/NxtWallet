using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Views
{
    public sealed partial class SendMoneyPage
    {
        private SendMoneyViewModel ViewModel => (SendMoneyViewModel) DataContext;

        public SendMoneyPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.OnNavigatedTo(e.Parameter as Contact);
        }
    }
}
