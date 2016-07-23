using Windows.UI.Xaml.Controls;

namespace NxtWallet.Views
{
    public sealed partial class BackupInfoDialog
    {
        private readonly INavigationService _navigationService;

        public BackupInfoDialog(INavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService;
        }

        private void BackupButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
            _navigationService.NavigateTo(NavigationPage.BackupSecretPhrasePage);
        }

        private void CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _navigationService.NavigateBack();
        }
    }
}
