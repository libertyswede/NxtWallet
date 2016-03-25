using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace NxtWallet
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var visible = (bool)value ? Visibility.Visible : Visibility.Collapsed;
            return visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}