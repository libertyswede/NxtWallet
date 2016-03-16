using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class SendMoneyPage
    {
        public SendMoneyViewModel ViewModel { get; } = new SendMoneyViewModel();

        public SendMoneyPage()
        {
            InitializeComponent();
        }
    }
}
