using System;
using Windows.UI.Xaml.Data;

namespace NxtWallet
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var timestamp = (DateTime) value;
            var formattedDate = timestamp.ToString(parameter.ToString());
            return formattedDate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
