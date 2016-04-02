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
    }
}
