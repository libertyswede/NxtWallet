using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;

namespace NxtWallet
{
    public class NetworkIndicatorTrigger : StateTriggerBase
    {
        public static readonly DependencyProperty IsOnlineProperty =
            DependencyProperty.Register("IsOnline", typeof(bool), typeof(NetworkIndicatorTrigger), null);
        public bool IsOnline
        {
            get { return (bool)GetValue(IsOnlineProperty); }
            set { SetValue(IsOnlineProperty, value); }
        }

        public NetworkIndicatorTrigger()
        {
            if (!GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                var nxtServer = ServiceLocator.Current.GetInstance<INxtServer>();
                IsOnline = nxtServer.IsOnline;
                SetActive(IsOnline);
                nxtServer.PropertyChanged += (sender, args) =>
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        IsOnline = nxtServer.IsOnline;
                        SetActive(IsOnline);
                    });
                };
            }
        }
    }
}
