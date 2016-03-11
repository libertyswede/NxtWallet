using System;
using Windows.UI.Xaml.Data;
using NxtLib;

namespace NxtWallet
{
    public class NxtAmountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var amount = Amount.CreateAmountFromNqt((long) value);
            var nxt = amount.Nxt.ToString("##.#########");
            return nxt;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
