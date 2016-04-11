using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using NxtWallet.Model;
using NxtWallet.ViewModel.Model;

namespace NxtWallet
{
    public interface IToastManager
    {
        void Register();
    }

    public class ToastManager : IToastManager
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IBackgroundRunner _backgroundRunner;

        public ToastManager(IWalletRepository walletRepository, IBackgroundRunner backgroundRunner)
        {
            _walletRepository = walletRepository;
            _backgroundRunner = backgroundRunner;
        }

        public void Register()
        {
            _backgroundRunner.TransactionAdded += (sender, transaction) =>
            {
                if (transaction.UserIsRecipient && _walletRepository.NotificationsEnabled)
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
