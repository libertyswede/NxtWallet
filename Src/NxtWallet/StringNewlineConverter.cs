using System;
using Windows.UI.Xaml.Data;

namespace NxtWallet
{
    public class StringNewlineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            const int maxLength = 50;

            if (value == null)
                return string.Empty;

            var theString = value.ToString();
            var firstNewLineIndex = theString.IndexOfAny(new[] { '\n', '\r' });
            int stringLengthCutoff = -1;

            if (theString.Length > maxLength && firstNewLineIndex == -1)
            {
                stringLengthCutoff = maxLength;
            }
            else if (theString.Length <= maxLength && firstNewLineIndex > -1)
            {
                stringLengthCutoff = firstNewLineIndex;
            }
            else if (theString.Length > maxLength && firstNewLineIndex > -1)
            {
                stringLengthCutoff = Math.Min(Math.Min(maxLength, theString.Length), firstNewLineIndex);
            }

            if (stringLengthCutoff > -1)
            {
                theString = theString.Substring(0, stringLengthCutoff);
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