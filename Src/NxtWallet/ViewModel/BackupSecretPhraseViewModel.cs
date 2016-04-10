using GalaSoft.MvvmLight.Command;
using NxtWallet.Model;

namespace NxtWallet.ViewModel
{
    public class BackupSecretPhraseViewModel
    {
        private readonly INavigationService _navigationService;
        public string SecretPhrase { get; set; }
        public RelayCommand ContinueCommand { get; }

        public BackupSecretPhraseViewModel(IWalletRepository walletRepository, INavigationService navigationService)
        {
            _navigationService = navigationService;
            SecretPhrase = walletRepository.SecretPhrase;
            ContinueCommand = new RelayCommand(Continue);
        }

        private void Continue()
        {
            _navigationService.NavigateTo(NavigationPage.BackupConfirmPage);
        }
    }
}