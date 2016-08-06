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
using System.Collections.Generic;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : ValidatableBindableBase
    {
        private readonly INxtServer _nxtServer;
        private readonly IWalletRepository _walletRepository;
        private readonly IAccountLedgerRepository _accountLedgerRepository;
        private readonly IContactRepository _contactRepository;
        private readonly INavigationService _navigationService;

        private string _recipientAddress;
        private string _amount;
        private string _message;
        private string _noteToSelfMessage;
        private string _info;
        private string _fee;
        private bool _isMessageEncryptionEnabled;
        private bool? _encryptMessage;
        private BinaryHexString _recipientPublicKey;

        [Required(ErrorMessage = "Recipient address is required")]
        [NxtRsAddress(ErrorMessage = "Incorrect recipient address")]
        public string RecipientAddress
        {
            get { return _recipientAddress; }
            set
            {
                if (!string.Equals(_recipientAddress, value))
                {
                    SetProperty(ref _recipientAddress, value);
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
                if (!string.Equals(_amount, value))
                {
                    SetProperty(ref _amount, value);
                    SendMoneyCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string Fee
        {
            get { return _fee; }
            set
            {
                if (!string.Equals(_fee, value))
                {
                    SetProperty(ref _fee, value);
                    SendMoneyCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (!string.Equals(_message, value))
                {
                    SetProperty(ref _message, value);
                    SendMoneyCommand.RaiseCanExecuteChanged();
                    CalculateFee();
                }
            }
        }

        public bool IsMessageEncryptionEnabled
        {
            get { return _isMessageEncryptionEnabled; }
            private set
            {
                if (_isMessageEncryptionEnabled != value)
                {
                    SetProperty(ref _isMessageEncryptionEnabled, value);
                }
            }
        }

        public bool? EncryptMessage
        {
            get { return _encryptMessage; }
            set
            {
                if (_encryptMessage != value)
                {
                    SetProperty(ref _encryptMessage, value);
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

        public string Info
        {
            get { return _info; }
            set { SetProperty(ref _info, value); }
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
            ClearFields();
        }

        private void ClearFields()
        {
            DisableValidation();
            IsMessageEncryptionEnabled = false;
            EncryptMessage = true;
            RecipientAddress = string.Empty;
            Amount = string.Empty;
            Message = string.Empty;
            NoteToSelfMessage = string.Empty;
            Fee = "1.0 NXT";
            EnableValidation();
        }

        public void OnNavigatedTo(Contact contact)
        {
            if (contact != null)
            {
                RecipientAddress = contact.NxtAddressRs;
            }
        }

        private bool CanSendMoney()
        {
            return _nxtServer.IsOnline && FormHasValues() && ValidateProperties();
        }

        private bool FormHasValues()
        {
            return !string.IsNullOrEmpty(RecipientAddress) && !string.IsNullOrEmpty(Amount);
        }

        internal async Task<IEnumerable<Contact>> GetMatchingRecipients(string text)
        {
            return await _contactRepository.SearchContactsWithNameContainingText(text);
        }

        internal void DisableValidation()
        {
            Errors.IsValidationEnabled = false;
        }

        internal void EnableValidation()
        {
            Errors.IsValidationEnabled = true;
        }

        internal async void UpdateRecipientInfo()
        {
            if (string.IsNullOrEmpty(RecipientAddress) || Errors[nameof(RecipientAddress)].Any())
            {
                Info = string.Empty;
                return;
            }

            AccountReply account = null;
            Contact contact = null;
            await Task.Run(async () =>
            {
                try
                {
                    _recipientPublicKey = null;
                    account = await _nxtServer.GetAccountAsync(RecipientAddress);
                }
                catch (Exception)
                {
                    // Ignore
                }
                contact = await _contactRepository.GetContactAsync(RecipientAddress);
            });
            if (!_nxtServer.IsOnline)
            {
                Info = "Unable to get information about recipient in offline mode.";
                IsMessageEncryptionEnabled = false;
                EncryptMessage = false;
                return;
            }
            if (account == null)
            {
                Info = "The recipient account is an unknown account, meaning it has never had an incoming or outgoing transaction.";
                IsMessageEncryptionEnabled = false;
                EncryptMessage = false;
                return;
            }
            _recipientPublicKey = account.PublicKey;
            IsMessageEncryptionEnabled = true;

            var recipientInfo = "The recipient";
            var name = (contact != null) ? contact.Name : (!string.IsNullOrEmpty(account.Name)) ? account.Name : string.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                recipientInfo += $", {name}";
            }
            recipientInfo += $" has a balance of {account.Balance.Nxt.ToFormattedString(2)} NXT";
            Info = recipientInfo;
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
            if ((!IsMessageEncryptionEnabled || !EncryptMessage.Value) && !string.IsNullOrEmpty(Message))
            {
                var plainMessage = new CreateTransactionParameters.UnencryptedMessage(Message, true);
                return plainMessage;
            }
            return null;
        }

        private CreateTransactionParameters.AlreadyEncryptedMessage GetEncryptedMessage()
        {
            if ((IsMessageEncryptionEnabled && EncryptMessage.Value) && !string.IsNullOrEmpty(Message))
            {
                var localMessageService = new LocalMessageService();
                var nonce = localMessageService.CreateNonce();
                var encrypted = localMessageService.EncryptTextTo(_recipientPublicKey, Message, nonce,
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

        private async void SendMoney()
        {
            var sendMoneyDialogViewModel = (SendMoneyDialogViewModel)_navigationService.ShowDialog(NavigationDialog.SendMoney);

            try
            {
                await Task.Run(async () =>
                {
                    var amount = NxtLib.Amount.CreateAmountFromNxt(decimal.Parse(Amount));
                    var ledgerEntry = await _nxtServer.SendMoneyAsync(RecipientAddress, amount, GetPlainMessage(), 
                        GetEncryptedMessage(), GetEncryptedNoteToSelfMessage());

                    if ((IsMessageEncryptionEnabled && EncryptMessage.Value) && !string.IsNullOrEmpty(Message))
                    {
                        ledgerEntry.EncryptedMessage = Message;
                    }   
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
            ClearFields();
        }
    }
}
