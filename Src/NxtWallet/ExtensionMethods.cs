using System.Collections.Generic;
using System.Linq;
using NxtWallet.Model;
using NxtWallet.ViewModel;

namespace NxtWallet
{
    public static class ExtensionMethods
    {
        public static string ToFormattedString(this decimal amount)
        {
            var formatted = amount.ToString("##.00#######");
            return formatted;
        }

        public static IEnumerable<ITransaction> Except(this IEnumerable<ITransaction> transactions,
            IEnumerable<ViewModelTransaction> viewModelTransactions)
        {
            return transactions.Where(transaction => viewModelTransactions.Any(vmTransaction => vmTransaction.NxtId == transaction.GetTransactionId()));
        }
    }
}
