﻿using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using NxtWallet.Controls;
using NxtWallet.Model;
using NxtWallet.ViewModel;
using SendMoneyDialog = NxtWallet.Controls.SendMoneyDialog;

namespace NxtWallet
{
    public class Ioc
    {
        public OverviewViewModel OverviewViewModel => ServiceLocator.Current.GetInstance<OverviewViewModel>();
        public SendMoneyViewModel SendMoneyViewModel => ServiceLocator.Current.GetInstance<SendMoneyViewModel>();
        public TransactionDetailViewModel TransactionDetailViewModel => ServiceLocator.Current.GetInstance<TransactionDetailViewModel>();
        public TransactionListViewModel TransactionListViewModel => ServiceLocator.Current.GetInstance<TransactionListViewModel>();

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

            SimpleIoc.Default.Register<ISendMoneyDialog, SendMoneyDialog>();
            SimpleIoc.Default.Register<IBalanceCalculator, BalanceCalculator>();
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