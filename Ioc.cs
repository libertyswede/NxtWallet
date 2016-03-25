using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using NxtWallet.ViewModel;

namespace NxtWallet
{
    public class Ioc
    {
        public TransactionListViewModel TransactionListViewModel { get; } = SimpleIoc.Default.GetInstance<TransactionListViewModel>();
        public SendMoneyViewModel SendMoneyViewModel { get; } = SimpleIoc.Default.GetInstance<SendMoneyViewModel>();

        static Ioc()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<INxtServer, FakeNxtServer>();
                SimpleIoc.Default.Register<IWalletRepository, FakeWalletRepository>();
            }
            else
            {
                SimpleIoc.Default.Register<INxtServer, NxtServer>();
                SimpleIoc.Default.Register<IWalletRepository, WalletRepository>();
            }

            SimpleIoc.Default.Register<OverviewViewModel>();
            SimpleIoc.Default.Register<SendMoneyViewModel>();
            SimpleIoc.Default.Register<TransactionListViewModel>();
            SimpleIoc.Default.Register<TransactionDetailViewModel>();
        }

        public static void Register()
        {
            // empty by design, logic is in static constructor
        }
    }
}
