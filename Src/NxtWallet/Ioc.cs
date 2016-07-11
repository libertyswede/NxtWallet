using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using NxtLib;
using NxtWallet.Controls;
using NxtWallet.Core;
using NxtWallet.Core.Fakes;
using NxtWallet.ViewModel;
using NxtWallet.Views;
using NxtWallet.Core.Repositories;

namespace NxtWallet
{
    public class Ioc
    {
        public OverviewViewModel OverviewViewModel => ServiceLocator.Current.GetInstance<OverviewViewModel>();
        public SendMoneyViewModel SendMoneyViewModel => ServiceLocator.Current.GetInstance<SendMoneyViewModel>();
        public LedgerEntryDetailViewModel TransactionDetailViewModel => ServiceLocator.Current.GetInstance<LedgerEntryDetailViewModel>();
        public LedgerEntryListViewModel LedgerEntryListViewModel => ServiceLocator.Current.GetInstance<LedgerEntryListViewModel>();
        public SettingsViewModel SettingsViewModel => ServiceLocator.Current.GetInstance<SettingsViewModel>();
        public ReceiveMoneyViewModel ReceiveMoneyViewModel => ServiceLocator.Current.GetInstance<ReceiveMoneyViewModel>();
        public ContactsViewModel ContactsViewModel => ServiceLocator.Current.GetInstance<ContactsViewModel>();
        public BackupSecretPhraseViewModel BackupSecretPhraseViewModel => ServiceLocator.Current.GetInstance<BackupSecretPhraseViewModel>();
        public BackupConfirmViewModel BackupConfirmViewModel => ServiceLocator.Current.GetInstance<BackupConfirmViewModel>();

        static Ioc()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<INxtServer, FakeNxtServer>();
                SimpleIoc.Default.Register<IWalletRepository, FakeWalletRepository>();
                SimpleIoc.Default.Register<IAccountLedgerRepository, FakeAccountLedgerRepository>();
                SimpleIoc.Default.Register<IContactRepository, FakeContactRepository>();
                SimpleIoc.Default.Register<IToastManager, FakeToastManager>();
            }
            else
            {
                SimpleIoc.Default.Register<INxtServer, NxtServer>();
                SimpleIoc.Default.Register<IWalletRepository, WalletRepository>();
                SimpleIoc.Default.Register<IAccountLedgerRepository, AccountLedgerRepository>();
                SimpleIoc.Default.Register<IContactRepository, ContactRepository>();
                SimpleIoc.Default.Register<IToastManager, ToastManager>();
            }
            var repo = ServiceLocator.Current.GetInstance<IWalletRepository>();

            SimpleIoc.Default.Register(() => MapperConfig.Setup().CreateMapper());
            SimpleIoc.Default.Register<IAccountLedgerRunner, AccountLedgerRunner>();
            SimpleIoc.Default.Register<INavigationService, NavigationService>();
            SimpleIoc.Default.Register<ISendMoneyDialog, SendMoneyDialog>();
            SimpleIoc.Default.Register<IBackupInfoDialog, BackupInfoDialog>();
            SimpleIoc.Default.Register<IBackupDoneDialog, BackupDoneDialog>();
            SimpleIoc.Default.Register<IServiceFactory>(() => new ServiceFactory(repo.NxtServer));
            SimpleIoc.Default.Register<OverviewViewModel>();
            SimpleIoc.Default.Register<SendMoneyViewModel>();
            SimpleIoc.Default.Register<LedgerEntryListViewModel>();
            SimpleIoc.Default.Register<LedgerEntryDetailViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<ReceiveMoneyViewModel>();
            SimpleIoc.Default.Register<ContactsViewModel>();
            SimpleIoc.Default.Register<BackupSecretPhraseViewModel>();
            SimpleIoc.Default.Register<BackupConfirmViewModel>();
        }

        public static void Register()
        {
            // empty by design, logic is in static constructor
        }
    }
}
