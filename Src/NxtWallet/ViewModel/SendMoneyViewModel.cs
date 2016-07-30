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
using NxtLib.Local;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : ValidatableBindableBase
    {
        private readonly INxtServer _nxtServer;
        private readonly IWalletRepository _walletRepository;
        private readonly IAccountLedgerRepository _accountLedgerRepository;
        private readonly IContactRepository _contactRepository;
        private readonly INavigationService _navigationService;

        private string _recipient;
        private string _amount;
        private string _plainMessage;
        private string _encryptedMessage;
        private string _noteToSelfMessage;
        private string _recipientInfo;
        private string _fee;
        private bool _encryptedMessageEnabled;
        private BinaryHexString _recipientPublicKey;

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

        public string Fee
        {
            get { return _fee; }
            set
            {
                SetProperty(ref _fee, value);
                SendMoneyCommand.RaiseCanExecuteChanged();
            }
        }

        public string PlainMessage
        {
            get { return _plainMessage; }
            set
            {
                if (!string.Equals(_plainMessage, value))
                {
                    SetProperty(ref _plainMessage, value);
                    SendMoneyCommand.RaiseCanExecuteChanged();
                    CalculateFee();
                }
            }
        }

        public string EncryptedMessage
        {
            get { return _encryptedMessage; }
            set
            {
                if (!string.Equals(_encryptedMessage, value))
                {
                    SetProperty(ref _encryptedMessage, value);
                    SendMoneyCommand.RaiseCanExecuteChanged();
                    CalculateFee();
                }
            }
        }

        public bool EncryptedMessageEnabled
        {
            get { return _encryptedMessageEnabled; }
            set
            {
                if (_encryptedMessageEnabled != value)
                {
                    SetProperty(ref _encryptedMessageEnabled, value);
                    SendMoneyCommand.RaiseCanExecuteChanged();
                    CalculateFee();
                }
            }
        }

        public string NoteToSelfMessage
        {
            get { return _noteToSelfMessage; }
            set
            {
                if (!string.Equals(_noteToSelfMessage, value))
                {
                    SetProperty(ref _noteToSelfMessage, value);
                    SendMoneyCommand.RaiseCanExecuteChanged();
                    CalculateFee();
                }
            }
        }

        public string RecipientInfo
        {
            get { return _recipientInfo; }
            set { SetProperty(ref _recipientInfo, value); }
        }

        public RelayCommand SendMoneyCommand { get; }

        public SendMoneyViewModel(INxtServer nxtServer, IWalletRepository walletRepository,
            IAccountLedgerRepository accountLedgerRepository, IContactRepository contactRepository, 
            INavigationService navigationService)
        {
            _nxtServer = nxtServer;
            _walletRepository = walletRepository;
            _accountLedgerRepository = accountLedgerRepository;
            _contactRepository = contactRepository;
            _navigationService = navigationService;

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
                    _recipientPublicKey = null;
                    account = await _nxtServer.GetAccountAsync(Recipient);
                }
                catch (Exception)
                {
                    // Ignore
                }
                contact = await _contactRepository.GetContactAsync(Recipient);
            });

            if (account == null && _nxtServer.IsOnline)
            {
                RecipientInfo = "The recipient account is an unknown account, meaning it has never had an incoming or outgoing transaction.";
                EncryptedMessageEnabled = false;
                return;
            }
            EncryptedMessageEnabled = true;
            _recipientPublicKey = account.PublicKey;

            var recipientInfo = "The recipient";
            var name = (contact != null) ? contact.Name : (!string.IsNullOrEmpty(account.Name)) ? account.Name : string.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                recipientInfo += $", {name}";
            }
            recipientInfo += $" has a balance of {account.Balance.Nxt.ToFormattedString(2)} NXT";
            RecipientInfo = recipientInfo;
        }

        private void CalculateFee()
        {
            var fee = 1M;

            var plainMessage = GetPlainMessage();
            if (plainMessage != null)
            {
                // 0 NXT for first 1023 bytes data
                // After that, 0.1 NXT per 1024 byte data
                fee += (plainMessage.Message.Length / 1024) * 0.1M;
            }
            
            var encryptedMessage = GetEncryptedMessage();
            if (encryptedMessage != null)
            {
                // 0 NXT for first 1024 bytes
                // After that, 0.1 NXT per 1024 byte encrypted data
                fee += ((encryptedMessage.Message.ToBytes().Count() - 1) / 1024) * 0.1M;
            }
            
            var encryptedNoteToSelf = GetEncryptedNoteToSelfMessage();
            if (encryptedNoteToSelf != null)
            {
                // 1 NXT for first 48 bytes of encrypted data (32 actual data + 16 bytes compression stuff)
                // 1 NXT for each additional encrypted 32 byte data
                fee += 1;
                fee += ((encryptedNoteToSelf.Message.ToBytes().ToArray().Length - 16 - 1) / 32) * 1M;
            }

            Fee = fee.ToFormattedString(1) + " NXT";
        }

        private CreateTransactionParameters.UnencryptedMessage GetPlainMessage()
        {
            if (!string.IsNullOrEmpty(PlainMessage))
            {
                var plainMessage = new CreateTransactionParameters.UnencryptedMessage(PlainMessage, true);
                return plainMessage;
            }
            return null;
        }

        private CreateTransactionParameters.AlreadyEncryptedMessage GetEncryptedMessage()
        {
            if (EncryptedMessageEnabled && !string.IsNullOrEmpty(EncryptedMessage))
            {
                var localMessageService = new LocalMessageService();
                var nonce = localMessageService.CreateNonce();
                var encrypted = localMessageService.EncryptTextTo(_recipientPublicKey, EncryptedMessage, nonce,
                    true, _walletRepository.SecretPhrase);
                var encryptedMessage = new CreateTransactionParameters.AlreadyEncryptedMessage(encrypted, nonce, true, true, true);
                return encryptedMessage;
            }
            return null;
        }

        private CreateTransactionParameters.AlreadyEncryptedMessageToSelf GetEncryptedNoteToSelfMessage()
        {
            if (!string.IsNullOrEmpty(NoteToSelfMessage))
            {
                var localMessageService = new LocalMessageService();
                var nonce = localMessageService.CreateNonce();
                var encryptedToSelf = localMessageService.EncryptTextTo(_walletRepository.NxtAccountWithPublicKey.PublicKey,
                    NoteToSelfMessage, nonce, true, _walletRepository.SecretPhrase);
                var encryptedMessageToSelf = new CreateTransactionParameters.AlreadyEncryptedMessageToSelf(encryptedToSelf, nonce, true, true);
                return encryptedMessageToSelf;
            }
            return null;
        }

        public void OnNavigatedTo(Contact contact)
        {
            Errors.IsValidationEnabled = false;
            Recipient = string.Empty;
            Amount = string.Empty;
            PlainMessage = string.Empty;
            EncryptedMessage = string.Empty;
            NoteToSelfMessage = string.Empty;
            EncryptedMessageEnabled = true;
            Fee = "1.0 NXT";

            Errors.IsValidationEnabled = true;
            if (contact != null)
            {
                Recipient = contact.NxtAddressRs;
            }
        }

        private async void SendMoney()
        {
            var sendMoneyDialogViewModel = (SendMoneyDialogViewModel)_navigationService.ShowDialog(NavigationDialog.SendMoney);

            try
            {
                await Task.Run(async () =>
                {
                    var amount = NxtLib.Amount.CreateAmountFromNxt(decimal.Parse(Amount));
                    var ledgerEntry = await _nxtServer.SendMoneyAsync(Recipient, amount, GetPlainMessage(), 
                        GetEncryptedMessage(), GetEncryptedNoteToSelfMessage());

                    ledgerEntry.NqtBalance = _walletRepository.NqtBalance + ledgerEntry.NqtAmount + ledgerEntry.NqtFee;
                    await _accountLedgerRepository.AddLedgerEntryAsync(ledgerEntry);
                    await _walletRepository.UpdateBalanceAsync(ledgerEntry.NqtBalance);
                });
                sendMoneyDialogViewModel.SetDone();
            }
            catch (NxtException e)
            {
                if (e.Message == "Not enough funds")
                {
                    sendMoneyDialogViewModel.SetError(e.Message);
                }
            }
        }
    }
}
