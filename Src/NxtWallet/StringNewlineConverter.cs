using System;
using Windows.UI.Xaml.Data;

namespace NxtWallet
{
    public class StringNewlineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var theString = value.ToString();
            var newLineIndex = theString.IndexOfAny(new[] {'\n', '\r'});
            if (newLineIndex != -1)
            {
                theString = theString.Substring(0, newLineIndex);
                theString = theString.TrimEnd() + " ...";
            }
            return theString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}