using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using NxtWallet.ViewModel;

namespace NxtWallet
{
    public class Ioc
    {
        public static IWalletRepository WalletRepository => ServiceLocator.Current.GetInstance<IWalletRepository>();
        public static INxtServer NxtServer => ServiceLocator.Current.GetInstance<INxtServer>();
        public static OverviewViewModel OverviewViewModel => ServiceLocator.Current.GetInstance<OverviewViewModel>();
        public static SendMoneyViewModel SendMoneyViewModel => ServiceLocator.Current.GetInstance<SendMoneyViewModel>();

        static Ioc()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IWalletRepository, WalletRepository>();
            SimpleIoc.Default.Register<INxtServer, NxtServer>();
            SimpleIoc.Default.Register<OverviewViewModel>();
            SimpleIoc.Default.Register<SendMoneyViewModel>();
        }
    }
}
