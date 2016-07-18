using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NxtWallet.Controls;
using NxtWallet.Core.Models;
using NxtWallet.Core;
using NxtWallet.Core.Repositories;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : ViewModelBase
    {
        private readonly INxtServer _nxtServer;
        private readonly IWalletRepository _walletRepository;
        private readonly IAccountLedgerRepository _accountLedgerRepository;
        private readonly ISendMoneyDialog _sendMoneyDialog;

        private string _recipient;
        private string _amount;
        private string _message;

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

        public RelayCommand SendMoneyCommand { get; }

        public SendMoneyViewModel(INxtServer nxtServer, IWalletRepository walletRepository,
            IAccountLedgerRepository accountLedgerRepository, ISendMoneyDialog sendMoneyDialog)
        {
            _nxtServer = nxtServer;
            _walletRepository = walletRepository;
            _accountLedgerRepository = accountLedgerRepository;
            _sendMoneyDialog = sendMoneyDialog;
            SendMoneyCommand = new RelayCommand(SendMoney);
            nxtServer.PropertyChanged += (sender, args) => SendMoneyCommand.CanExecute(_nxtServer.IsOnline);
        }

        public void OnNavigatedTo(Contact contact)
        {
            Recipient = contact?.NxtAddressRs;
            Amount = "";
            Message = "";
        }

        private async void SendMoney()
        {
            // ReSharper disable once UnusedVariable
            var ignore = _sendMoneyDialog.ShowAsync();
            await Task.Run(async () =>
            {
                // TODO: Could be a problem with different decimal separator signs in different regions
                var amount = decimal.Parse(Amount);
                var ledgerEntry = await _nxtServer.SendMoneyAsync(Recipient, NxtLib.Amount.CreateAmountFromNxt(amount), Message);
                SetBalance(ledgerEntry);
                await _accountLedgerRepository.AddLedgerEntryAsync(ledgerEntry);
                await _walletRepository.UpdateBalanceAsync(ledgerEntry.NqtBalance);
                //await Task.Delay(5000); // For testing purposes
            });
            _sendMoneyDialog.Hide();
        }

        private void SetBalance(LedgerEntry ledgerEntry)
        {
            var newBalanceNqt = _walletRepository.NqtBalance + ledgerEntry.NqtAmount + ledgerEntry.NqtFee;
            ledgerEntry.NqtBalance = newBalanceNqt;
        }
    }
}
