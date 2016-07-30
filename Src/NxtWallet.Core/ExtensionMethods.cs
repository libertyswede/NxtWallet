namespace NxtWallet.Core
{
    public static class ExtensionMethods
    {
        public static string ToFormattedString(this decimal amount)
        {
            var formatted = amount.ToString("#,##0.00#######;;");
            return formatted;
        }

        public static string ToFormattedString(this decimal amount, int decimalCount)
        {
            var decimals = new string('0', decimalCount);
            var formatted = amount.ToString($"#,##0.{decimals};;");
            return formatted;
        }

        public static decimal NqtToNxt(this long nqtAmount)
        {
            var amount = NxtLib.Amount.CreateAmountFromNqt(nqtAmount);
            return amount.Nxt;
        }

        public static long NxtToNqt(this decimal nxtAmount)
        {
            var amount = NxtLib.Amount.CreateAmountFromNxt(nxtAmount);
            return amount.Nqt;
        }
    }
}
