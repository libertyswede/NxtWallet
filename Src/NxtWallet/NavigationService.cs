using Windows.UI.Xaml;

namespace NxtWallet
{
    public enum NavigationPage
    {
        SendMoneyPage
    }

    public interface INavigationService
    {
        void NavigateTo(NavigationPage navigationPage, object parameter = null);
    }

    public class NavigationService : INavigationService
    {
        private readonly AppShell _appShell;

        public NavigationService()
        {
            _appShell = (AppShell)Window.Current.Content;
        }

        public void NavigateTo(NavigationPage navigationPage, object parameter = null)
        {
            _appShell.Navigate(navigationPage, parameter);
        }
    }
}