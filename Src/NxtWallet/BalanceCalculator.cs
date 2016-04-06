using System.Collections.Generic;
using System.Linq;
using NxtWallet.ViewModel.Model;

namespace NxtWallet
{
    public interface IBalanceCalculator
    {
        IEnumerable<TransactionModel> Calculate(IReadOnlyList<TransactionModel> newTransactions, IReadOnlyList<TransactionModel> allTransactions);
    }

    public class BalanceCalculator : IBalanceCalculator
    {
        public IEnumerable<TransactionModel> Calculate(IReadOnlyList<TransactionModel> newTransactions, IReadOnlyList<TransactionModel> allTransactions)
        {
            var allOrderedTransactions = allTransactions.OrderBy(t => t.Timestamp).ToList();
            var firstNewTransaction = newTransactions.OrderBy(t => t.Timestamp).First();
            UpdateTransactionBalance(firstNewTransaction, allTransactions);
            var updatedTransactions = UpdateSubsequentTransactionBalances(firstNewTransaction, allOrderedTransactions);
            updatedTransactions = updatedTransactions.Except(newTransactions);
            return updatedTransactions;
        }

        private IEnumerable<TransactionModel> UpdateSubsequentTransactionBalances(TransactionModel viewModelTransaction, IList<TransactionModel> allTransactions)
        {
            var updatedTransactions = new HashSet<TransactionModel>();

            foreach (var subsequentTransaction in GetSubsequentTransactions(viewModelTransaction, allTransactions))
            {
                UpdateTransactionBalance(subsequentTransaction, allTransactions);
                updatedTransactions.Add(subsequentTransaction);
            }

            return updatedTransactions;
        }

        private void UpdateTransactionBalance(TransactionModel transaction, IEnumerable<TransactionModel> allTransactions)
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

        private static IEnumerable<TransactionModel> GetSubsequentTransactions(TransactionModel transaction,
            IEnumerable<TransactionModel> allTransactions)
        {
            return allTransactions.Where(t => t.Timestamp.CompareTo(transaction.Timestamp) > 0).ToList();
        }

        private static TransactionModel GetPreviousTransaction(TransactionModel transaction,
            IEnumerable<TransactionModel> allTransactions)
        {
            return allTransactions
                .LastOrDefault(t => t.Timestamp.CompareTo(transaction.Timestamp) < 0);
        }
    }
}
