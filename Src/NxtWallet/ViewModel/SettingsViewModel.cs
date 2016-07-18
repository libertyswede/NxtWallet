using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NxtWallet.Core;
using NxtWallet.Core.Repositories;

namespace NxtWallet.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly INxtServer _nxtServer;
        private readonly IAccountLedgerRepository _accountLedgerRepository;
        private string _serverAddress;
        private string _readOnlyAddress;
        private bool? _isNotificationsEnabled;
        private string _secretPhrase;

        public string ServerAddress
        {
            get { return _serverAddress; }
            set { Set(ref _serverAddress, value); }
        }

        public bool? IsNotificationsEnabled
        {
            get { return _isNotificationsEnabled; }
            set { Set(ref _isNotificationsEnabled, value); }
        }

        public string ReadOnlyAddress
        {
            get { return _readOnlyAddress; }
            set { Set(ref _readOnlyAddress, value); }
        }

        public string SecretPhrase
        {
            get { return _secretPhrase; }
            set { Set(ref _secretPhrase, value); }
        }

        public RelayCommand SaveCommand { get; }

        public SettingsViewModel(IWalletRepository walletRepository, INxtServer nxtServer, IAccountLedgerRepository accountLedgerRepository)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;
            _accountLedgerRepository = accountLedgerRepository;
            SaveCommand = new RelayCommand(Save);
            ServerAddress = _walletRepository.NxtServer;
            IsNotificationsEnabled = _walletRepository.NotificationsEnabled;
            ReadOnlyAddress = _walletRepository.IsReadOnlyAccount ? _walletRepository.NxtAccount.AccountRs : string.Empty;
            SecretPhrase = _walletRepository.SecretPhrase;
        }

        private async void Save()
        {
            _nxtServer.UpdateNxtServer(_serverAddress);
            await Task.Run(async () =>
            {
                await _walletRepository.UpdateNxtServerAsync(_serverAddress);
                await _walletRepository.UpdateNotificationsEnabledAsync(IsNotificationsEnabled ?? true);
            });
            if (SecretPhrase != _walletRepository.SecretPhrase)
            {
                await Task.Run(async () =>
                {
                    await _accountLedgerRepository.DeleteAllLedgerEntriesAsync();
                    await _walletRepository.UpdateSecretPhraseAsync(SecretPhrase);
                    await _walletRepository.UpdateLastLedgerEntryBlockIdAsync(NxtLib.Local.Constants.GenesisBlockId);
                });
            }
            //if (ReadOnlyAddress != _walletRepository.NxtAccount.AccountRs)
            //{
            //    await Task.Run(async () =>
            //    {
            //        await _accountLedgerRepository.DeleteAllLedgerEntriesAsync();
            //        await _walletRepository.UpdateReadOnlyNxtAccountAsync(ReadOnlyAddress);
            //        await _walletRepository.UpdateLastLedgerEntryBlockIdAsync(NxtLib.Local.Constants.GenesisBlockId);
            //    });
            //}
        }
    }
}
