using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class TransactionDetailPage
    {
        public TransactionDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var transaction = (ViewModelTransaction) e.Parameter;
        }
    }
}
