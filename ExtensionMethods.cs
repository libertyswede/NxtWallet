using NxtLib;

namespace NxtWallet
{
    public static class ExtensionMethods
    {
        public static string ToFormattedString(this Amount amount)
        {
            var formatted = amount.Nxt.ToString("##.#########");
            return formatted;
        }
    }
}
