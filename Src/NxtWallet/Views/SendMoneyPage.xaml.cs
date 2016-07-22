using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;
using NxtWallet.Core.Models;
using Microsoft.Practices.ServiceLocation;

namespace NxtWallet.Views
{
    public sealed partial class SendMoneyPage
    {
        private SendMoneyViewModel ViewModel => (SendMoneyViewModel) DataContext;

        public SendMoneyPage()
        {
            InitializeComponent();
            ServiceLocator.Current.GetInstance<ISendMoneyDialog>(); // init dialog
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.OnNavigatedTo(e.Parameter as Contact);
        }
    }
}
