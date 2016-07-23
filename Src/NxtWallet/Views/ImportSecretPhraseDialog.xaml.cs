using GalaSoft.MvvmLight.Messaging;
using NxtWallet.ViewModel;
using Windows.UI.Xaml.Controls;

namespace NxtWallet.Views
{
    public sealed partial class ImportSecretPhraseDialog : ContentDialog
    {
        private bool _showing;

        private ImportSecretPhraseDialogViewModel ViewModel => (ImportSecretPhraseDialogViewModel)DataContext;

        public ImportSecretPhraseDialog()
        {
            InitializeComponent();
            ViewModel.SecretPhrase = string.Empty;
            Messenger.Default.Register<string>(this, ViewModel, (message) => Hide());
        }

        public new void Hide()
        {
            base.Hide();
            Messenger.Default.Unregister<string>(this, ViewModel);
        }

        private void SecretPhrase_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.SecretPhrase = ((TextBox)sender).Text;
        }

        private void CancelClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Hide();
        }
    }
}
