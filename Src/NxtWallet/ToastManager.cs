using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using NxtWallet.Repositories.Model;
using NxtWallet.Core.Models;
using NxtWallet.Core;

namespace NxtWallet
{
    public interface IToastManager
    {
        void Register();
    }

    public class ToastManager : IToastManager
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IAccountLedgerRunner _accountLedgerRunner;

        public ToastManager(IWalletRepository walletRepository, IAccountLedgerRunner accountLedgerRunner)
        {
            _walletRepository = walletRepository;
            _accountLedgerRunner = accountLedgerRunner;
        }

        public void Register()
        {
            _accountLedgerRunner.TransactionAdded += (sender, transaction) =>
            {
                if (transaction.UserIsTransactionRecipient && _walletRepository.NotificationsEnabled)
                    PopNewTransactionToast(transaction);
            };
        }

        private static void PopNewTransactionToast(Transaction transaction)
        {
            var message = string.IsNullOrEmpty(transaction.Message) ? string.Empty : $"\nMessage: {transaction.Message}";
            var from = transaction.ContactListAccountFrom ?? transaction.AccountFrom;

            var xmlToast =  "<toast launch=\"app-defined-string\">" +
                                "<visual>" +
                                "<binding template =\"ToastGeneric\">" +
                                    "<text>New NXT transaction</text>" +
                                    "<text>" +
                                    $"You received {transaction.FormattedAmount} NXT from {from}.\n" + 
                                    $"Your new balance is {transaction.FormattedBalance} NXT." +
                                    $"{message}" + 
                                    "</text>" +
                                "</binding>" +
                                "</visual>" +
                            "</toast>";

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlToast);
            var toastNotification = new ToastNotification(xmlDocument);
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            toastNotifier.Show(toastNotification);
        }
    }
}
