using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Ioc;

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
                SetActive(true);
                var nxtServer = SimpleIoc.Default.GetInstance<INxtServer>();
                IsOnline = nxtServer.IsOnline;
                nxtServer.PropertyChanged += (sender, args) => IsOnline = nxtServer.IsOnline;
            }
        }
    }
}
