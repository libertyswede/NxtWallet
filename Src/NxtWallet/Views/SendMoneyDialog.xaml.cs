using GalaSoft.MvvmLight.Messaging;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public interface ISendMoneyDialog : IDialog
    {
    }

    public sealed partial class SendMoneyDialog : ISendMoneyDialog
    {
        private SendMoneyDialogViewModel ViewModel => (SendMoneyDialogViewModel)DataContext;
        private bool _showing;

        public SendMoneyDialog()
        {
            Messenger.Default.Register<SendMoneyDialogMessage>(this, (message) => TryShow());
            InitializeComponent();
        }

        private void TryShow()
        {
            if (!_showing)
            {
                _showing = true;
                var ignore = ShowAsync();
            }
        }

        private void Close_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _showing = false;
            Hide();
        }
    }
}
