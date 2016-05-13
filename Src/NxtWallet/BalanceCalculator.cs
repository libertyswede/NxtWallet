using System.Collections.Generic;
using System.Linq;
using NxtWallet.ViewModel.Model;

namespace NxtWallet
{
    public interface IBalanceCalculator
    {
        IEnumerable<T> Calculate<T>(IReadOnlyList<T> newEntries, IReadOnlyList<T> allEntries) where T : ILedgerEntry;
        bool BalanceEqualsLastTransactionBalance(IReadOnlyList<Transaction> newTransactions, IReadOnlyList<Transaction> knownTransactions, 
            HashSet<Transaction> updatedTransactions, long balanceResult);
    }

    public class BalanceCalculator : IBalanceCalculator
    {
        public IEnumerable<T> Calculate<T>(IReadOnlyList<T> newEntries, IReadOnlyList<T> allEntries) where T : ILedgerEntry
        {
            var allOrderedEntries = allEntries.OrderBy(t => t.GetOrder()).ToList();
            var firstNewEntry = newEntries.OrderBy(t => t.GetOrder()).First();
            UpdateEntryBalance(firstNewEntry, allOrderedEntries);
            var updatedEntries = UpdateSubsequentEntryBalances(firstNewEntry, allOrderedEntries);
            updatedEntries = updatedEntries.Except(newEntries);
            return updatedEntries;
        }

        public bool BalanceEqualsLastTransactionBalance(IReadOnlyList<Transaction> newTransactions, IReadOnlyList<Transaction> knownTransactions,
            HashSet<Transaction> updatedTransactions, long balanceResult)
        {
            newTransactions = newTransactions.Except(knownTransactions).ToList();
            var allOrderedTransactions = newTransactions.Union(knownTransactions).OrderBy(t => t.Timestamp).ToList();

            if (newTransactions.Any())
            {
                var updated = Calculate(newTransactions, allOrderedTransactions);
                updated.ToList().ForEach(t => updatedTransactions.Add(t));
            }
            var lastTxBalance = allOrderedTransactions.LastOrDefault()?.NqtBalance ?? 0;
            var unconfirmedSum = allOrderedTransactions.Where(t => !t.IsConfirmed)
                .Sum(t => t.UserIsTransactionRecipient ? t.NqtAmount : -t.NqtAmount);
            var equals = balanceResult + unconfirmedSum == lastTxBalance;
            return equals;
        }

        private static IEnumerable<T> UpdateSubsequentEntryBalances<T>(T entry, IList<T> allOrderedEntries) where T : ILedgerEntry
        {
            var updatedEntries = new HashSet<T>();

            foreach (var subsequentEntry in GetSubsequentEntries(entry, allOrderedEntries))
            {
                UpdateEntryBalance(subsequentEntry, allOrderedEntries);
                updatedEntries.Add(subsequentEntry);
            }

            return updatedEntries;
        }

        private static void UpdateEntryBalance<T>(T entry, IList<T> allOrderedEntries) where T : ILedgerEntry
        {
            var previousBalance = GetPreviousEntry(entry, allOrderedEntries)?.GetBalance() ?? 0;

            if (entry.UserIsTransactionSender)
            {
                if (!entry.UserIsAmountRecipient)
                {
                    entry.SetBalance(previousBalance - (entry.GetAmount() + entry.GetFee()));
                }
                else
                {
                    entry.SetBalance(previousBalance + entry.GetAmount() - entry.GetFee());
                }
            }
            else
            {
                entry.SetBalance(previousBalance + entry.GetAmount());
            }
        }

        private static IEnumerable<T> GetSubsequentEntries<T>(T entry,
            IList<T> allOrderedEntries) where T : ILedgerEntry
        {
            return allOrderedEntries.Skip(allOrderedEntries.IndexOf(entry) + 1).ToList();
        }

        private static T GetPreviousEntry<T>(T entry,
            IList<T> allOrderedEntries) where T : ILedgerEntry
        {
            var index = allOrderedEntries.IndexOf(entry);
            return index > 0 ? allOrderedEntries[index - 1] : default(T);
        }
    }
}
