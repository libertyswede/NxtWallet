using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NxtWallet.Views
{
    public interface IBackupInfoDialog : IDialog
    {
    }

    public sealed partial class BackupInfoDialog : IBackupInfoDialog
    {
        public BackupInfoDialog()
        {
            InitializeComponent();
        }

        private void BackupButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
            var appShell = (AppShell)Window.Current.Content;
            appShell.Navigate(NavigationPage.BackupSecretPhrasePage, null);
        }

        private void CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var appShell = (AppShell)Window.Current.Content;
            appShell.AppFrame.GoBack();
        }
    }
}
