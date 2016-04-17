using System.Collections.Generic;
using System.Linq;
using NxtWallet.ViewModel.Model;

namespace NxtWallet
{
    public interface IBalanceCalculator
    {
        IEnumerable<T> Calculate<T>(IReadOnlyList<T> newEntries, IReadOnlyList<T> allEntries) where T : ILedgerEntry;
        bool BalanceEqualsLastTransactionBalance(IReadOnlyList<Transaction> newTransactions, IReadOnlyList<Transaction> knownTransactions, List<Transaction> updatedTransactions, long balanceResult);
    }

    public class BalanceCalculator : IBalanceCalculator
    {
        public IEnumerable<T> Calculate<T>(IReadOnlyList<T> newEntries, IReadOnlyList<T> allEntries) where T : ILedgerEntry
        {
            var allOrderedEntries = allEntries.OrderBy(t => t.GetOrder()).ToList();
            var firstNewEntry = newEntries.OrderBy(t => t.GetOrder()).First();
            UpdateEntryBalance(firstNewEntry, allEntries);
            var updatedEntries = UpdateSubsequentEntryBalances(firstNewEntry, allOrderedEntries);
            updatedEntries = updatedEntries.Except(newEntries);
            return updatedEntries;
        }

        public bool BalanceEqualsLastTransactionBalance(IReadOnlyList<Transaction> newTransactions, IReadOnlyList<Transaction> knownTransactions,
            List<Transaction> updatedTransactions, long balanceResult)
        {
            newTransactions = newTransactions.Except(knownTransactions).ToList();
            var allOrderedTransactions = newTransactions.Union(knownTransactions).OrderBy(t => t.Timestamp).ToList();

            if (newTransactions.Any())
            {
                var updated = Calculate(newTransactions, allOrderedTransactions);
                updatedTransactions.AddRange(updated);
            }
            var lastTxBalance = allOrderedTransactions.LastOrDefault()?.NqtBalance ?? 0;
            var unconfirmedSum = allOrderedTransactions.Where(t => !t.IsConfirmed)
                .Sum(t => t.UserIsRecipient ? t.NqtAmount : -t.NqtAmount);
            var equals = balanceResult + unconfirmedSum == lastTxBalance;
            return equals;
        }

        private IEnumerable<T> UpdateSubsequentEntryBalances<T>(T entry, IList<T> allEntries) where T : ILedgerEntry
        {
            var updatedEntries = new HashSet<T>();

            foreach (var subsequentEntry in GetSubsequentEntries(entry, allEntries))
            {
                UpdateEntryBalance(subsequentEntry, allEntries);
                updatedEntries.Add(subsequentEntry);
            }

            return updatedEntries;
        }

        private void UpdateEntryBalance<T>(T entry, IEnumerable<T> allEntries) where T : ILedgerEntry
        {
            var previousBalance = GetPreviousEntry(entry, allEntries)?.GetBalance() ?? 0;

            if (entry.UserIsSender())
            {
                entry.SetBalance(previousBalance - (entry.GetAmount() + entry.GetFee()));
            }
            else
            {
                entry.SetBalance(previousBalance + entry.GetAmount());
            }
        }

        private static IEnumerable<T> GetSubsequentEntries<T>(T entry,
            IEnumerable<T> allEntries) where T : ILedgerEntry
        {
            return allEntries.Where(t => t.GetOrder() > entry.GetOrder()).ToList();
        }

        private static T GetPreviousEntry<T>(T entry,
            IEnumerable<T> allEntries) where T : ILedgerEntry
        {
            return allEntries
                .LastOrDefault(t => t.GetOrder() < entry.GetOrder());
        }
    }
}
