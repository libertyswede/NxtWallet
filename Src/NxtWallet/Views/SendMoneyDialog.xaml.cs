using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class SendMoneyDialog
    {
        internal SendMoneyDialogViewModel ViewModel => (SendMoneyDialogViewModel)DataContext;

        public SendMoneyDialog()
        {
            InitializeComponent();
            ViewModel.Init();
        }

        private void Close_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Hide();
        }
    }
}
