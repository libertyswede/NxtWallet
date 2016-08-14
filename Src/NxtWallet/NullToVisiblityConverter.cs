using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace NxtWallet
{
    public class NullToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var visibility = value == null ? Visibility.Collapsed : Visibility.Visible;
            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
