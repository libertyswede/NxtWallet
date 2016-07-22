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
            Messenger.Default.Register<ImportSecretPhraseMessage>(this, (message) => DoShow(message));
        }

        private void DoShow(ImportSecretPhraseMessage message)
        {
            if (message.MessageState == ImportSecretPhraseMessage.State.Import && _showing == false)
            {
                _showing = true;
                var ignore = ShowAsync();
            }
            else if (message.MessageState == ImportSecretPhraseMessage.State.Imported && _showing == true)
            {
                _showing = false;
                Hide();
            }
        }

        private void CancelClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.SecretPhrase = string.Empty;
            _showing = false;
            Hide();
        }

        private void SecretPhrase_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.SecretPhrase = ((TextBox)sender).Text;
        }
    }
}
