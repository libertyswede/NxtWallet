using Windows.UI.Xaml.Controls;
using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class BackupConfirmPage
    {
        private BackupConfirmViewModel ViewModel => (BackupConfirmViewModel)DataContext;

        public BackupConfirmPage()
        {
            InitializeComponent();
        }

        private void TextBox_OnPaste(object sender, TextControlPasteEventArgs e)
        {
            e.Handled = true;
        }
    }
}
