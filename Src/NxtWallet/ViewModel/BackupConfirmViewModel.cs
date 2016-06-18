using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NxtWallet.Core.Model;
using NxtWallet.Views;

namespace NxtWallet.ViewModel
{
    public class BackupConfirmViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IBackupDoneDialog _backupDoneDialog;

        private string _secretPhraseConfirmation;

        public string SecretPhraseConfirmation
        {
            get { return _secretPhraseConfirmation; }
            set { Set(ref _secretPhraseConfirmation, value); }
        }

        public ICommand ConfirmCommand { get; }

        public BackupConfirmViewModel(IWalletRepository walletRepository, IBackupDoneDialog backupDoneDialog)
        {
            _walletRepository = walletRepository;
            _backupDoneDialog = backupDoneDialog;
            ConfirmCommand = new RelayCommand(ConfirmSecretPhrase);
        }

        public async void ConfirmSecretPhrase()
        {
            if (string.Equals(_walletRepository.SecretPhrase, SecretPhraseConfirmation))
            {
                await Task.Run(async () => await _walletRepository.UpdateBackupCompleted(true));
                await _backupDoneDialog.ShowAsync();
            }
        }
    }
}