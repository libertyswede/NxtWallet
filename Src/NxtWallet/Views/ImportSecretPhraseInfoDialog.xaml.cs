using GalaSoft.MvvmLight.Messaging;
using NxtWallet.ViewModel;
using Windows.UI.Xaml.Controls;

namespace NxtWallet.Views
{
    public sealed partial class ImportSecretPhraseInfoDialog : ContentDialog
    {
        bool _showing;

        public ImportSecretPhraseInfoDialog()
        {
            Messenger.Default.Register<ImportSecretPhraseMessage>(this, (message) => DoShow());
            InitializeComponent();
        }

        private void DoShow()
        {
            if (!_showing)
            {
                _showing = true;
                var ignore = ShowAsync();
            }
        }

        private void ContinueButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        private void CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }
    }
}
