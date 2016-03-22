using NxtLib;

namespace NxtWallet
{
    public static class ExtensionMethods
    {
        public static string ToFormattedString(this decimal amount)
        {
            var formatted = amount.ToString("##.00#######");
            return formatted;
        }
    }
}
