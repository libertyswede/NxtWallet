namespace NxtWallet
{
    public static class ExtensionMethods
    {
        public static string ToFormattedString(this decimal amount)
        {
            var formatted = amount.ToString("##.00#######");
            return formatted;
        }

        public static decimal NqtToNxt(this long nqtAmount)
        {
            var nxtAmount = nqtAmount/100000000m;
            return nxtAmount;
        }
    }
}
