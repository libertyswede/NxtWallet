using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : ViewModelBase
    {
        private readonly INxtServer _nxtServer;
        private readonly IWalletRepository _walletRepository;

        private string _recipient;
        private string _amount;
        private string _message;
        private bool _isSendingMoney;

        public string Recipient
        {
            get { return _recipient; }
            set { Set(ref _recipient, value); }
        }

        public string Amount
        {
            get { return _amount; }
            set { Set(ref _amount, value); }
        }

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        public bool IsSendingMoney
        {
            get { return _isSendingMoney; }
            set { Set(ref _isSendingMoney, value); }
        }

        public RelayCommand SendMoneyCommand { get; }

        [PreferredConstructor]
        public SendMoneyViewModel(INxtServer nxtServer, IWalletRepository walletRepository)
        {
            IsSendingMoney = false;
            _nxtServer = nxtServer;
            _walletRepository = walletRepository;
            SendMoneyCommand = new RelayCommand(SendMoney, () => nxtServer.IsOnline);
            nxtServer.PropertyChanged += (sender, args) => SendMoneyCommand.RaiseCanExecuteChanged();
        }

        private async void SendMoney()
        {
            IsSendingMoney = true;
            await Task.Run(async () =>
            {
                decimal amount;
                decimal.TryParse(Amount, out amount);
                var transaction = await _nxtServer.SendMoneyAsync(Recipient, NxtLib.Amount.CreateAmountFromNxt(amount), Message);
                await _walletRepository.SaveTransactionAsync(transaction);
                //await Task.Delay(5000); // For testing purposes
            });
            IsSendingMoney = false;
        }
    }
}
