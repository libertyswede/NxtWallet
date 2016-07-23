using NxtWallet.Views;
using System;
using Windows.UI.Xaml;

namespace NxtWallet
{
    public enum NavigationPage
    {
        BackupConfirmPage,
        BackupSecretPhrasePage,
        ReceiveMoneyPage,
        SendMoneyPage
    }

    public enum NavigationDialog
    {
        SendMoney,
        BackupInfo,
        BackupDone,
        ImportSecretPhraseInfo,
        ImportSecretPhrase
    }

    public interface INavigationService
    {
        void NavigateBack();
        void NavigateTo(NavigationPage navigationPage, object parameter = null);
        object ShowDialog(NavigationDialog navigationDialog);
    }

    public class NavigationService : INavigationService
    {
        private readonly AppShell _appShell;

        public NavigationService()
        {
            _appShell = (AppShell)Window.Current.Content;
        }

        public void NavigateBack()
        {
            _appShell.AppFrame.GoBack();
        }

        public void NavigateTo(NavigationPage navigationPage, object parameter = null)
        {
            _appShell.Navigate(navigationPage, parameter);
        }

        public object ShowDialog(NavigationDialog navigationDialog)
        {
            if (navigationDialog == NavigationDialog.SendMoney)
            {
                var dialog = new SendMoneyDialog();
                var ignore = dialog.ShowAsync();
                return dialog.ViewModel;
            }
            if (navigationDialog == NavigationDialog.BackupInfo)
            {
                var dialog = new BackupInfoDialog(this);
                var ignore = dialog.ShowAsync();
                return null;
            }
            if (navigationDialog == NavigationDialog.BackupDone)
            {
                var dialog = new BackupDoneDialog();
                var ignore = dialog.ShowAsync();
                return null;
            }
            if (navigationDialog == NavigationDialog.ImportSecretPhraseInfo)
            {
                var dialog = new ImportSecretPhraseInfoDialog(this);
                var ignore = dialog.ShowAsync();
                return null;
            }
            if (navigationDialog == NavigationDialog.ImportSecretPhrase)
            {
                var dialog = new ImportSecretPhraseDialog();
                var ignore = dialog.ShowAsync();
                return null;
            }

            throw new ArgumentException("Unknown dialog type", nameof(navigationDialog));
        }
    }
}