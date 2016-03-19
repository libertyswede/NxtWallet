using Windows.UI.Xaml;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class SendMoneyPage
    {
        public SendMoneyViewModel ViewModel { get; } = new SendMoneyViewModel(((App)Application.Current).WalletRepository);

        public SendMoneyPage()
        {
            InitializeComponent();
        }
    }
}
