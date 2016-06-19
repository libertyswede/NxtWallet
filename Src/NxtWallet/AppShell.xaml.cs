using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using NxtWallet.Controls;
using NxtWallet.Views;
using NxtWallet.Repositories.Model;

namespace NxtWallet
{
    public sealed partial class AppShell
    {
        public static AppShell Current;
        private readonly IWalletRepository _walletRepository;
        private readonly List<NavMenuItem> _navlist;

        /// <summary>
        /// Initializes a new instance of the AppShell, sets the static 'Current' reference,
        /// adds callbacks for Back requests and changes in the SplitView's DisplayMode, and
        /// provide the nav menu list with the data to display.
        /// </summary>
        public AppShell(IWalletRepository walletRepository)
        {
            InitializeComponent();
            _walletRepository = walletRepository;

            _navlist = BuildNavigationList();

            Loaded += (sender, args) =>
            {
                Current = this;
                TogglePaneButton.Focus(FocusState.Programmatic);
            };

            RootSplitView.RegisterPropertyChangedCallback(
                SplitView.DisplayModeProperty,
                (s, a) =>
                {
                    // Ensure that we update the reported size of the TogglePaneButton when the SplitView's
                    // DisplayMode changes.
                    CheckTogglePaneButtonSizeChanged();
                });

            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;

            NavMenuList.ItemsSource = _navlist;
        }

        private List<NavMenuItem> BuildNavigationList()
        {
            var navigationList = new List<NavMenuItem>(new[] {
                new NavMenuItem
                {
                    ImageSource = "Assets\\Menu\\home-48x48.png",
                    Label = "Overview",
                    DestPage = typeof(OverviewPage)
                },
                new NavMenuItem
                {
                    ImageSource = "Assets\\Menu\\send-money-48x48.png",
                    Label = "Send NXT",
                    Height = 22,
                    Width = 22,
                    DestPage = typeof(SendMoneyPage)
                },
                new NavMenuItem
                {
                    ImageSource = "Assets\\Menu\\receive-money-48x48.png",
                    Label = "Receive NXT",
                    Height = 22,
                    Width = 22,
                    DestPage = typeof(ReceiveMoneyPage)
                },
                new NavMenuItem
                {
                    ImageSource = "Assets\\Menu\\contacts-48x48.png",
                    Label = "Contacts",
                    DestPage = typeof(ContactsPage)
                },
                new NavMenuItem
                {
                    ImageSource = "Assets\\Menu\\settings-48x48.png",
                    Label = "Settings",
                    DestPage = typeof(SettingsPage)
                }
            });
            if (_walletRepository.IsReadOnlyAccount)
            {
                navigationList.RemoveAt(1);
            }
            return navigationList;
        }

        public void Navigate(NavigationPage navigationPage, object parameter)
        {
            switch (navigationPage)
            {
                case NavigationPage.BackupConfirmPage:
                    MainFrame.Navigate(typeof (BackupConfirmPage), parameter);
                    break;
                case NavigationPage.BackupSecretPhrasePage:
                    MainFrame.Navigate(typeof(BackupSecretPhrasePage), parameter);
                    break;
                case NavigationPage.ReceiveMoneyPage:
                    NavMenuList.SetSelectedItem((ListViewItem)NavMenuList.ContainerFromIndex(2));
                    MainFrame.Navigate(typeof (ReceiveMoneyPage), parameter);
                    break;
                case NavigationPage.SendMoneyPage:
                    NavMenuList.SetSelectedItem((ListViewItem)NavMenuList.ContainerFromIndex(1));
                    MainFrame.Navigate(typeof (SendMoneyPage), parameter);
                    break;
            }
        }

        public Frame AppFrame => MainFrame;

        /// <summary>
        /// Default keyboard focus movement for any unhandled keyboarding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppShell_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var direction = FocusNavigationDirection.None;
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.GamepadDPadLeft:
                case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                case Windows.System.VirtualKey.NavigationLeft:
                    direction = FocusNavigationDirection.Left;
                    break;
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.GamepadDPadRight:
                case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                case Windows.System.VirtualKey.NavigationRight:
                    direction = FocusNavigationDirection.Right;
                    break;

                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadDPadUp:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                case Windows.System.VirtualKey.NavigationUp:
                    direction = FocusNavigationDirection.Up;
                    break;

                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.GamepadDPadDown:
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                case Windows.System.VirtualKey.NavigationDown:
                    direction = FocusNavigationDirection.Down;
                    break;
            }

            if (direction != FocusNavigationDirection.None)
            {
                var control = FocusManager.FindNextFocusableElement(direction) as Control;
                if (control != null)
                {
                    control.Focus(FocusState.Programmatic);
                    e.Handled = true;
                }
            }
        }

        #region BackRequested Handlers

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled && AppFrame.CanGoBack)
            {
                e.Handled = true;
                AppFrame.GoBack();
            }
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Navigate to the Page for the selected <paramref name="listViewItem"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="listViewItem"></param>
        private void NavMenuList_ItemInvoked(object sender, ListViewItem listViewItem)
        {
            var item = (NavMenuItem)((NavMenuListView)sender).ItemFromContainer(listViewItem);

            if (item?.DestPage != null &&
                item.DestPage != AppFrame.CurrentSourcePageType)
            {
                AppFrame.Navigate(item.DestPage, item.Arguments);
            }
        }

        /// <summary>
        /// Ensures the nav menu reflects reality when navigation is triggered outside of
        /// the nav menu buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                var item = (from p in _navlist where p.DestPage == e.SourcePageType select p).SingleOrDefault();
                if (item == null && AppFrame.BackStackDepth > 0)
                {
                    // In cases where a page drills into sub-pages then we'll highlight the most recent
                    // navigation menu item that appears in the BackStack
                    foreach (var entry in AppFrame.BackStack.Reverse())
                    {
                        item = (from p in _navlist where p.DestPage == entry.SourcePageType select p).SingleOrDefault();
                        if (item != null)
                            break;
                    }
                }

                var container = (ListViewItem)NavMenuList.ContainerFromItem(item);

                // While updating the selection state of the item prevent it from taking keyboard focus.  If a
                // user is invoking the back button via the keyboard causing the selected nav menu item to change
                // then focus will remain on the back button.
                if (container != null) container.IsTabStop = false;
                NavMenuList.SetSelectedItem(container);
                if (container != null) container.IsTabStop = true;
            }
        }

        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            // After a successful navigation set keyboard focus to the loaded page
            var page = e.Content as Page;
            if (page != null)
            {
                var control = page;
                control.Loaded += Page_Loaded;
            }

            // Update the Back button depending on whether we can go Back.
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppFrame.CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Page)sender).Focus(FocusState.Programmatic);
            ((Page)sender).Loaded -= Page_Loaded;
            CheckTogglePaneButtonSizeChanged();
        }

        #endregion

        public Rect TogglePaneButtonRect
        {
            get;
            private set;
        }

        /// <summary>
        /// An event to notify listeners when the hamburger button may occlude other content in the app.
        /// The custom "PageHeader" user control is using this.
        /// </summary>
        public event TypedEventHandler<AppShell, Rect> TogglePaneButtonRectChanged;

        /// <summary>
        /// Callback when the SplitView's Pane is toggled open or close.  When the Pane is not visible
        /// then the floating hamburger may be occluding other content in the app unless it is aware.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TogglePaneButton_Checked(object sender, RoutedEventArgs e)
        {
            CheckTogglePaneButtonSizeChanged();
        }

        /// <summary>
        /// Check for the conditions where the navigation pane does not occupy the space under the floating
        /// hamburger button and trigger the event.
        /// </summary>
        private void CheckTogglePaneButtonSizeChanged()
        {
            if (RootSplitView.DisplayMode == SplitViewDisplayMode.Inline ||
                RootSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                var transform = TogglePaneButton.TransformToVisual(this);
                var rect = transform.TransformBounds(new Rect(0, 0, TogglePaneButton.ActualWidth, TogglePaneButton.ActualHeight));
                TogglePaneButtonRect = rect;
            }
            else
            {
                TogglePaneButtonRect = new Rect();
            }

            var handler = TogglePaneButtonRectChanged;
            handler?.DynamicInvoke(this, TogglePaneButtonRect);
        }

        /// <summary>
        /// Enable accessibility on each nav menu item by setting the AutomationProperties.Name on each container
        /// using the associated Label of each item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavMenuItemContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue && args.Item is NavMenuItem)
            {
                args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((NavMenuItem)args.Item).Label);
            }
            else
            {
                args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
            }
        }
    }
}
