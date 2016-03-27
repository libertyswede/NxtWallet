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
            updatedTransactions = updatedTransactions.Except(newTransactions.Select(t => t.Transaction));
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
                updatedTransactions.Add(subsequentTransaction.Transaction);
            }

            return updatedTransactions;
        }

        private void UpdateTransactionBalance(ViewModelTransaction viewModelTransaction, IEnumerable<ViewModelTransaction> allTransactions)
        {
            var previousBalance = GetPreviousTransaction(viewModelTransaction, allTransactions)?.Transaction?.NqtBalance ?? 0;
            var transaction = viewModelTransaction.Transaction;

            if (transaction.IsReceived(_walletRepository.NxtAccount.AccountRs))
            {
                viewModelTransaction.SetBalance(previousBalance + transaction.NqtAmount);
            }
            else
            {
                viewModelTransaction.SetBalance(previousBalance - (transaction.NqtAmount + transaction.NqtFeeAmount));
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
