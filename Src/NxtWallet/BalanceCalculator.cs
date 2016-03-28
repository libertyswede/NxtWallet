using System.Collections.Generic;
using System.Linq;
using NxtWallet.Model;

namespace NxtWallet
{
    public interface IBalanceCalculator
    {
        IEnumerable<ITransaction> Calculate(IReadOnlyList<ITransaction> newTransactions, IReadOnlyList<ITransaction> allTransactions);
    }

    public class BalanceCalculator : IBalanceCalculator
    {
        private readonly IWalletRepository _walletRepository;

        public BalanceCalculator(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public IEnumerable<ITransaction> Calculate(IReadOnlyList<ITransaction> newTransactions, IReadOnlyList<ITransaction> allTransactions)
        {
            var firstNewTransaction = newTransactions.OrderBy(t => t.Timestamp).First();
            UpdateTransactionBalance(firstNewTransaction, allTransactions);
            var updatedTransactions = UpdateSubsequentTransactionBalances(firstNewTransaction, allTransactions);
            updatedTransactions = updatedTransactions.Except(newTransactions);
            return updatedTransactions;
        }

        private IEnumerable<ITransaction> UpdateSubsequentTransactionBalances(ITransaction viewModelTransaction, IReadOnlyList<ITransaction> allTransactions)
        {
            var updatedTransactions = new HashSet<ITransaction>();

            foreach (var subsequentTransaction in GetSubsequentTransactions(viewModelTransaction, allTransactions))
            {
                UpdateTransactionBalance(subsequentTransaction, allTransactions);
                updatedTransactions.Add(subsequentTransaction);
            }

            return updatedTransactions;
        }

        private void UpdateTransactionBalance(ITransaction transaction, IEnumerable<ITransaction> allTransactions)
        {
            var previousBalance = GetPreviousTransaction(transaction, allTransactions)?.NqtBalance ?? 0;

            if (transaction.IsReceived(_walletRepository.NxtAccount.AccountRs))
            {
                transaction.NqtBalance = previousBalance + transaction.NqtAmount;
            }
            else
            {
                transaction.NqtBalance = previousBalance - (transaction.NqtAmount + transaction.NqtFeeAmount);
            }
        }

        private static IEnumerable<ITransaction> GetSubsequentTransactions(ITransaction transaction,
            IEnumerable<ITransaction> allTransactions)
        {
            return allTransactions.Where(t => t.Timestamp.CompareTo(transaction.Timestamp) > 0).ToList();
        }

        private static ITransaction GetPreviousTransaction(ITransaction transaction,
            IEnumerable<ITransaction> allTransactions)
        {
            return allTransactions.FirstOrDefault(t => t.Timestamp.CompareTo(transaction.Timestamp) < 0);
        }
    }
}
