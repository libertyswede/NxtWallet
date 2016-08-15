using Windows.UI.Xaml;

namespace NxtWallet
{
    public class ControlWidthTrigger : StateTriggerBase
    {
        private bool isActive;

        public static readonly DependencyProperty ElementProperty =
            DependencyProperty.Register("Element", typeof(FrameworkElement), typeof(ControlWidthTrigger),
            new PropertyMetadata(null, OnElementPropertyChanged));

        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.Register("MinWidth", typeof(double), typeof(ControlWidthTrigger),
            new PropertyMetadata(null, OnWidthPropertyChanged));

        public FrameworkElement Element
        {
            get { return (FrameworkElement)GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }

        public double MinWidth
        {
            get { return (double)GetValue(MinWidthProperty); }
            set { SetValue(MinWidthProperty, value); }
        }
        
        public bool IsActive
        {
            get { return isActive; }
            private set
            {
                if (isActive != value)
                {
                    isActive = value;
                    SetActive(value);
                }
            }
        }

        private static void OnElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (ControlWidthTrigger)d;
            obj.UpdateTrigger();
        }

        private static void OnWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (ControlWidthTrigger)d;
            obj.IsActive = obj.Element?.Width < obj.MinWidth;
        }

        private void UpdateTrigger()
        {
            Element.SizeChanged += Element_SizeChanged;
        }

        private void Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            IsActive = Element.ActualWidth > MinWidth;
        }
    }
}
