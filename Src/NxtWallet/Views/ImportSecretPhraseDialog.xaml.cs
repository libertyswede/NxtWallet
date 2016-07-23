using NxtWallet.ViewModel;
using Windows.UI.Xaml.Controls;

namespace NxtWallet.Views
{
    public sealed partial class ImportSecretPhraseDialog : ContentDialog
    {
        private ImportSecretPhraseDialogViewModel ViewModel => (ImportSecretPhraseDialogViewModel)DataContext;

        public ImportSecretPhraseDialog()
        {
            InitializeComponent();
            ViewModel.SecretPhrase = string.Empty;
        }

        private void SecretPhrase_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.SecretPhrase = ((TextBox)sender).Text;
        }
    }
}
