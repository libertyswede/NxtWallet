﻿using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using NxtWallet.Controls;
using NxtWallet.Core.Models;
using NxtWallet.Core;
using NxtWallet.Core.Repositories;
using Prism.Windows.Validation;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System;

namespace NxtWallet.ViewModel
{
    public class SendMoneyViewModel : ValidatableBindableBase
    {
        private readonly INxtServer _nxtServer;
        private readonly IWalletRepository _walletRepository;
        private readonly IAccountLedgerRepository _accountLedgerRepository;
        private readonly ISendMoneyDialog _sendMoneyDialog;

        private string _recipient;
        private string _amount;
        private string _message;

        [Required(ErrorMessage = "Recipient is required")]
        [NxtRsAddress(ErrorMessage = "Incorrect recipient address")]
        public string Recipient
        {
            get { return _recipient; }
            set { SetProperty(ref _recipient, value); }
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

        public string Message
        {
            get { return _message; }
            set
            {
                SetProperty(ref _message, value);
                SendMoneyCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SendMoneyCommand { get; }

        public SendMoneyViewModel(INxtServer nxtServer, IWalletRepository walletRepository,
            IAccountLedgerRepository accountLedgerRepository, ISendMoneyDialog sendMoneyDialog)
        {
            _nxtServer = nxtServer;
            _walletRepository = walletRepository;
            _accountLedgerRepository = accountLedgerRepository;
            _sendMoneyDialog = sendMoneyDialog;
            SendMoneyCommand = new RelayCommand(SendMoney, () => CanSendMoney());
            nxtServer.PropertyChanged += (sender, args) => SendMoneyCommand.RaiseCanExecuteChanged();
            ErrorsChanged += (sender, args) => SendMoneyCommand.RaiseCanExecuteChanged();
        }

        private bool CanSendMoney()
        {
            return _nxtServer.IsOnline && !GetAllErrors().Any() && FormHasValues();
        }

        private bool FormHasValues()
        {
            return !string.IsNullOrEmpty(Recipient) && !string.IsNullOrEmpty(Amount);
        }

        public void OnNavigatedTo(Contact contact)
        {
            Errors.IsValidationEnabled = false;
            Recipient = string.Empty;
            Amount = string.Empty;
            Message = string.Empty;

            Errors.IsValidationEnabled = true;
            if (contact != null)
            {
                Recipient = contact.NxtAddressRs;
            }
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
