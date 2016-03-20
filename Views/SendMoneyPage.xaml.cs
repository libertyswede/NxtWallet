using Microsoft.Practices.ServiceLocation;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class SendMoneyPage
    {
        public SendMoneyViewModel ViewModel { get; } = ServiceLocator.Current.GetInstance<SendMoneyViewModel>();

        public SendMoneyPage()
        {
            InitializeComponent();
        }
    }
}
