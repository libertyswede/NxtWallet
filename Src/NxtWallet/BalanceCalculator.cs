using System.Collections.Generic;
using System.Linq;
using NxtWallet.ViewModel.Model;

namespace NxtWallet
{
    public interface IBalanceCalculator
    {
        IEnumerable<Transaction> Calculate(IReadOnlyList<Transaction> newTransactions, IReadOnlyList<Transaction> allTransactions);
        bool BalanceEqualsLastTransactionBalance(IEnumerable<Transaction> nxtTransactions, IReadOnlyList<Transaction> knownTransactions, List<Transaction> updatedTransactions, long balanceResult);
    }

    public class BalanceCalculator : IBalanceCalculator
    {
        public IEnumerable<Transaction> Calculate(IReadOnlyList<Transaction> newTransactions, IReadOnlyList<Transaction> allTransactions)
        {
            var allOrderedTransactions = allTransactions.OrderBy(t => t.Timestamp).ToList();
            var firstNewTransaction = newTransactions.OrderBy(t => t.Timestamp).First();
            UpdateTransactionBalance(firstNewTransaction, allTransactions);
            var updatedTransactions = UpdateSubsequentTransactionBalances(firstNewTransaction, allOrderedTransactions);
            updatedTransactions = updatedTransactions.Except(newTransactions);
            return updatedTransactions;
        }

        public bool BalanceEqualsLastTransactionBalance(IEnumerable<Transaction> nxtTransactions, IReadOnlyList<Transaction> knownTransactions,
            List<Transaction> updatedTransactions, long balanceResult)
        {
            var newTransactions = nxtTransactions.Except(knownTransactions).ToList();
            var allOrderedTransactions = newTransactions.Union(knownTransactions).OrderBy(t => t.Timestamp).ToList();

            if (newTransactions.Any())
            {
                updatedTransactions.AddRange(Calculate(newTransactions, allOrderedTransactions));
            }
            var lastTxBalance = allOrderedTransactions.LastOrDefault()?.NqtBalance ?? 0;
            var unconfirmedSum = allOrderedTransactions.Where(t => !t.IsConfirmed)
                .Sum(t => t.UserIsRecipient ? t.NqtAmount : -t.NqtAmount);
            var equals = balanceResult + unconfirmedSum == lastTxBalance;
            return equals;
        }

        private IEnumerable<Transaction> UpdateSubsequentTransactionBalances(Transaction viewTransaction, IList<Transaction> allTransactions)
        {
            var updatedTransactions = new HashSet<Transaction>();

            foreach (var subsequentTransaction in GetSubsequentTransactions(viewTransaction, allTransactions))
            {
                UpdateTransactionBalance(subsequentTransaction, allTransactions);
                updatedTransactions.Add(subsequentTransaction);
            }

            return updatedTransactions;
        }

        private void UpdateTransactionBalance(Transaction transaction, IEnumerable<Transaction> allTransactions)
        {
            var previousBalance = GetPreviousTransaction(transaction, allTransactions)?.NqtBalance ?? 0;

            if (transaction.UserIsSender)
            {
                transaction.NqtBalance = previousBalance - (transaction.NqtAmount + transaction.NqtFee);
            }
            else
            {
                transaction.NqtBalance = previousBalance + transaction.NqtAmount;
            }
        }

        private static IEnumerable<Transaction> GetSubsequentTransactions(Transaction transaction,
            IEnumerable<Transaction> allTransactions)
        {
            return allTransactions.Where(t => t.Timestamp.CompareTo(transaction.Timestamp) > 0).ToList();
        }

        private static Transaction GetPreviousTransaction(Transaction transaction,
            IEnumerable<Transaction> allTransactions)
        {
            return allTransactions
                .LastOrDefault(t => t.Timestamp.CompareTo(transaction.Timestamp) < 0);
        }
    }
}
