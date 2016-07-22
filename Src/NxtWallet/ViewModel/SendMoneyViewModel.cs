using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using NxtWallet.Core.Models;
using NxtWallet.Core;
using NxtWallet.Core.Repositories;
using Prism.Windows.Validation;
using System.ComponentModel.DataAnnotations;
using System;
using NxtLib;
using System.Linq;
using NxtLib.Accounts;
using NxtWallet.Views;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : ValidatableBindableBase
    {
        private readonly INxtServer _nxtServer;
        private readonly IWalletRepository _walletRepository;
        private readonly IAccountLedgerRepository _accountLedgerRepository;
        private readonly ISendMoneyDialog _sendMoneyDialog;
        private readonly IContactRepository _contactRepository;

        private string _recipient;
        private string _amount;
        private string _message;
        private string _nrsErrorMessage;
        private string _recipientInfo;

        [Required(ErrorMessage = "Recipient is required")]
        [NxtRsAddress(ErrorMessage = "Incorrect recipient address")]
        public string Recipient
        {
            get { return _recipient; }
            set
            {
                if (!string.Equals(_recipient, value))
                {
                    SetProperty(ref _recipient, value);
                    UpdateRecipientInfo();
                }
            }
        }

        [Required(ErrorMessage = "Amount is required")]
        [Decimal(ErrorMessage = "Must be a valid decimal number")]
        [NxtAmount(ErrorMessage = "Must be a valid decimal number between 0 and 1000000000")]
        public string Amount
        {
            get { return _amount; }
            set
            {
                SetProperty(ref _amount, value);
                SendMoneyCommand.RaiseCanExecuteChanged();
            }
        }

        public string Fee { get; set; } = "1 NXT";

        public string Message
        {
            get { return _message; }
            set
            {
                SetProperty(ref _message, value);
                SendMoneyCommand.RaiseCanExecuteChanged();
            }
        }

        public string NrsErrorMessage
        {
            get { return _nrsErrorMessage; }
            set
            {
                SetProperty(ref _nrsErrorMessage, value);
                SendMoneyCommand.RaiseCanExecuteChanged();
            }
        }

        public string RecipientInfo
        {
            get { return _recipientInfo; }
            set { SetProperty(ref _recipientInfo, value); }
        }

        public RelayCommand SendMoneyCommand { get; }

        public SendMoneyViewModel(INxtServer nxtServer, IWalletRepository walletRepository,
            IAccountLedgerRepository accountLedgerRepository, ISendMoneyDialog sendMoneyDialog, 
            IContactRepository contactRepository)
        {
            _nxtServer = nxtServer;
            _walletRepository = walletRepository;
            _accountLedgerRepository = accountLedgerRepository;
            _sendMoneyDialog = sendMoneyDialog;
            _contactRepository = contactRepository;

            SendMoneyCommand = new RelayCommand(SendMoney, () => CanSendMoney());
            nxtServer.PropertyChanged += (sender, args) => SendMoneyCommand.RaiseCanExecuteChanged();
            ErrorsChanged += (sender, args) => SendMoneyCommand.RaiseCanExecuteChanged();
        }

        private bool CanSendMoney()
        {
            return _nxtServer.IsOnline && FormHasValues() && ValidateProperties();
        }

        private bool FormHasValues()
        {
            return !string.IsNullOrEmpty(Recipient) && !string.IsNullOrEmpty(Amount);
        }

        private async void UpdateRecipientInfo()
        {
            if (string.IsNullOrEmpty(Recipient) || Errors[nameof(Recipient)].Any())
            {
                RecipientInfo = string.Empty;
                return;
            }

            AccountReply account = null;
            Contact contact = null;
            await Task.Run(async () =>
            {
                try
                {
                    account = await _nxtServer.GetAccountAsync(Recipient);
                }
                catch (NxtException e)
                {
                    if (e.Message != "Unknown account")
                    {
                        throw;
                    }
                }
                contact = await _contactRepository.GetContactAsync(Recipient);
            });

            if (account == null)
            {
                RecipientInfo = "The recipient account is an unknown account, meaning it has never had an incoming or outgoing transaction.";
                return;
            }

            var recipientInfo = "The recipient";
            var name = (contact != null) ? contact.Name : (!string.IsNullOrEmpty(account.Name)) ? account.Name : string.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                recipientInfo += $", {name}";
            }
            recipientInfo += $" has a balance of {account.Balance.Nxt.ToFormattedStringTwoDecimals()} NXT";
            RecipientInfo = recipientInfo;
        }

        public void OnNavigatedTo(Contact contact)
        {
            Errors.IsValidationEnabled = false;
            Recipient = string.Empty;
            Amount = string.Empty;
            Message = string.Empty;
            NrsErrorMessage = string.Empty;

            Errors.IsValidationEnabled = true;
            if (contact != null)
            {
                Recipient = contact.NxtAddressRs;
            }
        }

        private async void SendMoney()
        {
            var ignore = _sendMoneyDialog.ShowAsync();
            try
            {
                await Task.Run(async () =>
                {
                    var amount = NxtLib.Amount.CreateAmountFromNxt(decimal.Parse(Amount));
                    var ledgerEntry = await _nxtServer.SendMoneyAsync(Recipient, amount, Message);
                    NrsErrorMessage = string.Empty;
                    ledgerEntry.NqtBalance = _walletRepository.NqtBalance + ledgerEntry.NqtAmount + ledgerEntry.NqtFee;
                    await _accountLedgerRepository.AddLedgerEntryAsync(ledgerEntry);
                    await _walletRepository.UpdateBalanceAsync(ledgerEntry.NqtBalance);
                    // await Task.Delay(5000);
                });
            }
            catch (NxtException e)
            {
                if (e.Message == "Not enough funds")
                {
                    NrsErrorMessage = e.Message;
                }
            }
            finally
            {
                _sendMoneyDialog.Hide();
            }
        }
    }
}
