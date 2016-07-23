using Windows.UI.Xaml.Controls;

namespace NxtWallet.Views
{
    public sealed partial class ImportSecretPhraseInfoDialog : ContentDialog
    {
        private readonly INavigationService _navigationService;

        public ImportSecretPhraseInfoDialog(INavigationService navigationService)
        {
            _navigationService = navigationService;
            InitializeComponent();
        }

        private void ContinueButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
            _navigationService.ShowDialog(NavigationDialog.ImportSecretPhrase);
        }

        private void CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }
    }
}
