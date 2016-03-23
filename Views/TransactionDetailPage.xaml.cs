using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Ioc;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class TransactionDetailPage
    {
        public TransactionDetailViewModel ViewModel { get; } = SimpleIoc.Default.GetInstance<TransactionDetailViewModel>();

        public TransactionDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Transaction = (ViewModelTransaction) e.Parameter;
        }
    }
}
