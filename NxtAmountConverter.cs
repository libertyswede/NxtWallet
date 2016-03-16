using System;
using Windows.UI.Xaml.Data;
using NxtLib;

namespace NxtWallet
{
    public class NxtAmountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var nqtAmount = (long) value;
            var amount = Amount.CreateAmountFromNqt(Math.Abs(nqtAmount));
            var nxt = amount.Nxt.ToString("##.#########");
            if (nqtAmount < 0)
                nxt = "-" + nxt;
            return nxt;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
