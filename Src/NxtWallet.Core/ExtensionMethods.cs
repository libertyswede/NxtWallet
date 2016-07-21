namespace NxtWallet.Core
{
    public static class ExtensionMethods
    {
        public static string ToFormattedString(this decimal amount)
        {
            var formatted = amount.ToString("#,##0.00#######;;");
            return formatted;
        }

        public static string ToFormattedStringTwoDecimals(this decimal amount)
        {
            var formatted = amount.ToString("#,##0.00;;");
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
