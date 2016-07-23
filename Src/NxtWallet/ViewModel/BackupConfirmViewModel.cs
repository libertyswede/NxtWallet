using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NxtWallet.Core.Repositories;

namespace NxtWallet.ViewModel
{
    public class BackupConfirmViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly INavigationService _navigationService;

        private string _secretPhraseConfirmation;

        public string SecretPhraseConfirmation
        {
            get { return _secretPhraseConfirmation; }
            set { Set(ref _secretPhraseConfirmation, value); }
        }

        public ICommand ConfirmCommand { get; }

        public BackupConfirmViewModel(IWalletRepository walletRepository, INavigationService navigationService)
        {
            _walletRepository = walletRepository;
            _navigationService = navigationService;
            ConfirmCommand = new RelayCommand(ConfirmSecretPhrase);
        }

        public async void ConfirmSecretPhrase()
        {
            if (string.Equals(_walletRepository.SecretPhrase, SecretPhraseConfirmation))
            {
                await Task.Run(async () => await _walletRepository.UpdateBackupCompletedAsync(true));
                _navigationService.ShowDialog(NavigationDialog.BackupDone);
            }
        }
    }
}