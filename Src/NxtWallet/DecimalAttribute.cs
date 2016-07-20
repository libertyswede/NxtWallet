using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NxtWallet
{
    public class DecimalAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var separator = Regex.Escape(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            var regex = new Regex($"^[0-9]+{separator}?[0-9]*$");
            var match = regex.Match(value.ToString());
            return match.Success;
        }
    }
}
