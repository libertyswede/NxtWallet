using Windows.UI.Xaml;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class SendMoneyPage
    {
        public SendMoneyViewModel ViewModel { get; } = Ioc.SendMoneyViewModel;

        public SendMoneyPage()
        {
            InitializeComponent();
        }
    }
}
