using System.Collections.Generic;
using System.Linq;
using NxtWallet.ViewModel.Model;

namespace NxtWallet
{
    public interface IBalanceCalculator
    {
        IEnumerable<Transaction> Calculate(IReadOnlyList<Transaction> newTransactions, IReadOnlyList<Transaction> allTransactions);
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

            if (transaction.UserIsRecipient)
            {
                transaction.NqtBalance = previousBalance + transaction.NqtAmount;
            }
            else
            {
                transaction.NqtBalance = previousBalance - (transaction.NqtAmount + transaction.NqtFee);
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
