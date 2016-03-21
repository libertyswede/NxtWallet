using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using NxtWallet.ViewModel;

namespace NxtWallet
{
    public class Ioc
    {
        public static TransactionListViewModel TransactionListViewModel { get; } = SimpleIoc.Default.GetInstance<TransactionListViewModel>();

        static Ioc()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IWalletRepository, WalletRepository>();
            SimpleIoc.Default.Register<INxtServer, NxtServer>();
            SimpleIoc.Default.Register<OverviewViewModel>();
            SimpleIoc.Default.Register<SendMoneyViewModel>();
            SimpleIoc.Default.Register<TransactionListViewModel>();
        }

        public static void Register()
        {
            // empty by design, logic is in static constructor
        }
    }
}
