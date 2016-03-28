using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using NxtWallet.Model;

namespace NxtWallet
{
    public interface IBackgroundRunner
    {
        Task Run(CancellationToken token);

        event TransactionHandler TransactionConfirmationUpdated;
        event TransactionHandler TransactionBalanceUpdated;
        event TransactionHandler TransactionAdded;
    }

    public delegate void TransactionHandler(IBackgroundRunner sender, ITransaction transaction);

    public class BackgroundRunner : IBackgroundRunner
    {
        private readonly INxtServer _nxtServer;
        private readonly IWalletRepository _walletRepository;
        private readonly IBalanceCalculator _balanceCalculator;

        public event TransactionHandler TransactionConfirmationUpdated;
        public event TransactionHandler TransactionBalanceUpdated;
        public event TransactionHandler TransactionAdded;

        public BackgroundRunner(INxtServer nxtServer, IWalletRepository walletRepository, IBalanceCalculator balanceCalculator)
        {
            _nxtServer = nxtServer;
            _walletRepository = walletRepository;
            _balanceCalculator = balanceCalculator;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var knownTransactions = (await _walletRepository.GetAllTransactionsAsync()).ToList();
                var nxtTransactions = (await _nxtServer.GetTransactionsAsync()).ToList();

                var newTransactions = nxtTransactions.Except(knownTransactions).ToList();
                var updatedTransactions = GetTransactionsWithUpdatedConfirmation(knownTransactions, nxtTransactions, newTransactions);
                
                await HandleUpdatedTransactions(updatedTransactions);
                await HandleNewTransactions(newTransactions, knownTransactions);

                await Task.Delay(30000, token);
            }
        }

        private async Task HandleUpdatedTransactions(List<ITransaction> updatedTransactions)
        {
            await _walletRepository.UpdateTransactionsAsync(updatedTransactions);
            updatedTransactions.ForEach(OnTransactionConfirmationUpdated);
        }

        private async Task HandleNewTransactions(List<ITransaction> newTransactions, List<ITransaction> knownTransactions)
        {
            if (newTransactions.Any())
            {
                var allTransactions = knownTransactions.Union(newTransactions).OrderBy(t => t.Timestamp).ToList();
                var updated = _balanceCalculator.Calculate(newTransactions, allTransactions).ToList();
                await _walletRepository.SaveTransactionsAsync(newTransactions);
                await _walletRepository.UpdateTransactionsAsync(updated);

                updated.ForEach(OnTransactionBalanceUpdated);
                newTransactions.ForEach(OnTransactionAdded);
            }
        }

        private static List<ITransaction> GetTransactionsWithUpdatedConfirmation(IEnumerable<ITransaction> knownTransactions, 
            IEnumerable<ITransaction> nxtTransactions, IEnumerable<ITransaction> newTransactions)
        {
            var updatedTransactions = knownTransactions
                .Where(t => t.IsConfirmed == false)
                .Except(nxtTransactions.Where(t => t.IsConfirmed == false))
                .Except(newTransactions)
                .ToList();

            updatedTransactions.ForEach(t => t.IsConfirmed = true);
            return updatedTransactions;
        }

        protected virtual void OnTransactionConfirmationUpdated(ITransaction transaction)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => TransactionConfirmationUpdated?.Invoke(this, transaction));
        }

        protected virtual void OnTransactionBalanceUpdated(ITransaction transaction)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => TransactionBalanceUpdated?.Invoke(this, transaction));
        }

        protected virtual void OnTransactionAdded(ITransaction transaction)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => TransactionAdded?.Invoke(this, transaction));
        }
    }
}