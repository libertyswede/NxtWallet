using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using NxtWallet.Model;
using NxtWallet.ViewModel.Model;

namespace NxtWallet
{
    public interface IBackgroundRunner
    {
        Task Run(CancellationToken token);

        event TransactionHandler TransactionConfirmationUpdated;
        event TransactionHandler TransactionBalanceUpdated;
        event TransactionHandler TransactionAdded;
    }

    public delegate void TransactionHandler(IBackgroundRunner sender, Transaction transaction);

    public class BackgroundRunner : IBackgroundRunner
    {
        private readonly INxtServer _nxtServer;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBalanceCalculator _balanceCalculator;

        public event TransactionHandler TransactionConfirmationUpdated;
        public event TransactionHandler TransactionBalanceUpdated;
        public event TransactionHandler TransactionAdded;

        public BackgroundRunner(INxtServer nxtServer, ITransactionRepository transactionRepository, IBalanceCalculator balanceCalculator)
        {
            _nxtServer = nxtServer;
            _transactionRepository = transactionRepository;
            _balanceCalculator = balanceCalculator;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var knownTransactions = (await _transactionRepository.GetAllTransactionsAsync()).ToList();
                var nxtTransactions = (await _nxtServer.GetTransactionsAsync()).ToList();

                var newTransactions = nxtTransactions.Except(knownTransactions).ToList();
                var updatedTransactions = GetTransactionsWithUpdatedConfirmation(knownTransactions, nxtTransactions, newTransactions);
                
                await HandleUpdatedTransactions(updatedTransactions);
                await HandleNewTransactions(newTransactions, knownTransactions);

                await Task.Delay(10000, token);
            }
        }

        private async Task HandleUpdatedTransactions(List<Transaction> updatedTransactions)
        {
            await _transactionRepository.UpdateTransactionsAsync(updatedTransactions);
            updatedTransactions.ForEach(OnTransactionConfirmationUpdated);
        }

        private async Task HandleNewTransactions(List<Transaction> newTransactions, List<Transaction> knownTransactions)
        {
            if (newTransactions.Any())
            {
                var allTransactions = knownTransactions.Union(newTransactions).OrderBy(t => t.Timestamp).ToList();
                var updated = _balanceCalculator.Calculate(newTransactions, allTransactions).ToList();
                await _transactionRepository.SaveTransactionsAsync(newTransactions);
                await _transactionRepository.UpdateTransactionsAsync(updated);

                updated.ForEach(OnTransactionBalanceUpdated);
                newTransactions.ForEach(OnTransactionAdded);
            }
        }

        private static List<Transaction> GetTransactionsWithUpdatedConfirmation(IEnumerable<Transaction> knownTransactions, 
            IEnumerable<Transaction> nxtTransactions, IEnumerable<Transaction> newTransactions)
        {
            var updatedTransactions = knownTransactions
                .Where(t => t.IsConfirmed == false)
                .Except(nxtTransactions.Where(t => t.IsConfirmed == false))
                .Except(newTransactions)
                .ToList();

            updatedTransactions.ForEach(t => t.IsConfirmed = true);
            return updatedTransactions;
        }

        protected virtual void OnTransactionConfirmationUpdated(Transaction transaction)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => TransactionConfirmationUpdated?.Invoke(this, transaction));
        }

        protected virtual void OnTransactionBalanceUpdated(Transaction transaction)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => TransactionBalanceUpdated?.Invoke(this, transaction));
        }

        protected virtual void OnTransactionAdded(Transaction transaction)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => TransactionAdded?.Invoke(this, transaction));
        }
    }
}