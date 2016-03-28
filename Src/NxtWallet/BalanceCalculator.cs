using System.Collections.Generic;
using System.Linq;
using NxtWallet.Model;
using NxtWallet.ViewModel;

namespace NxtWallet
{
    public interface IBalanceCalculator
    {
        IEnumerable<ITransaction> Calculate(IReadOnlyList<ViewModelTransaction> newTransactions, IReadOnlyList<ViewModelTransaction> allTransactions);
        void Calculate(ViewModelTransaction newTransaction, IReadOnlyList<ViewModelTransaction> allTransactions);
    }

    public class BalanceCalculator : IBalanceCalculator
    {
        private readonly IWalletRepository _walletRepository;

        public BalanceCalculator(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public IEnumerable<ITransaction> Calculate(IReadOnlyList<ViewModelTransaction> newTransactions, IReadOnlyList<ViewModelTransaction> allTransactions)
        {
            var firstNewTransaction = newTransactions.OrderBy(t => t.Timestamp).First();
            UpdateTransactionBalance(firstNewTransaction, allTransactions);
            var updatedTransactions = UpdateSubsequentTransactionBalances(firstNewTransaction, allTransactions);
            updatedTransactions = updatedTransactions.Except(newTransactions);
            return updatedTransactions;
        }

        public void Calculate(ViewModelTransaction newTransaction, IReadOnlyList<ViewModelTransaction> allTransactions)
        {
            Calculate(new List<ViewModelTransaction> {newTransaction}, allTransactions);
        }

        private IEnumerable<ITransaction> UpdateSubsequentTransactionBalances(ViewModelTransaction viewModelTransaction, IReadOnlyList<ViewModelTransaction> allTransactions)
        {
            var updatedTransactions = new HashSet<ITransaction>();

            foreach (var subsequentTransaction in GetSubsequentTransactions(viewModelTransaction, allTransactions))
            {
                UpdateTransactionBalance(subsequentTransaction, allTransactions);
                updatedTransactions.Add(new Transaction(subsequentTransaction));
            }

            return updatedTransactions;
        }

        private void UpdateTransactionBalance(ViewModelTransaction transaction, IEnumerable<ViewModelTransaction> allTransactions)
        {
            var previousBalance = GetPreviousTransaction(transaction, allTransactions)?.NqtBalance ?? 0;

            if (transaction.IsReceived(_walletRepository.NxtAccount.AccountRs))
            {
                transaction.SetBalance(previousBalance + transaction.NqtAmount);
            }
            else
            {
                transaction.SetBalance(previousBalance - (transaction.NqtAmount + transaction.NqtFee));
            }
        }

        private static IEnumerable<ViewModelTransaction> GetSubsequentTransactions(ViewModelTransaction transaction,
            IEnumerable<ViewModelTransaction> allTransactions)
        {
            return allTransactions.Where(t => t.Timestamp.CompareTo(transaction.Timestamp) > 0).ToList();
        }

        private static ViewModelTransaction GetPreviousTransaction(ViewModelTransaction transaction,
            IEnumerable<ViewModelTransaction> allTransactions)
        {
            return allTransactions.FirstOrDefault(t => t.Timestamp.CompareTo(transaction.Timestamp) < 0);
        }
    }
}
