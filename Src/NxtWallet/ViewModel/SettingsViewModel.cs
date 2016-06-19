using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NxtWallet.Repositories.Model;
using NxtWallet.Core;

namespace NxtWallet.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly INxtServer _nxtServer;
        private string _serverAddress;
        private bool? _isNotificationsEnabled;

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

        public RelayCommand SaveCommand { get; }

        public SettingsViewModel(IWalletRepository walletRepository, INxtServer nxtServer)
        {
            _walletRepository = walletRepository;
            _nxtServer = nxtServer;
            SaveCommand = new RelayCommand(Save);
            ServerAddress = _walletRepository.NxtServer;
            IsNotificationsEnabled = _walletRepository.NotificationsEnabled;
        }

        private async void Save()
        {
            _nxtServer.UpdateNxtServer(_serverAddress);
            await Task.Run(async () =>
            {
                await _walletRepository.UpdateNxtServerAsync(_serverAddress);
                await _walletRepository.UpdateNotificationsEnabledAsync(IsNotificationsEnabled ?? true);
            });
        }
    }
}
