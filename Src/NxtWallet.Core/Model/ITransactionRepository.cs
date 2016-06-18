using System.Collections.Generic;
using System.Threading.Tasks;
using NxtWallet.Core.ViewModel.Model;

namespace NxtWallet.Core.Model
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task SaveTransactionAsync(Transaction transaction);
        Task UpdateTransactionsAsync(IEnumerable<Transaction> transactionModels);
        Task SaveTransactionsAsync(IEnumerable<Transaction> transactions);
        Task<bool> HasOutgoingTransactionAsync();
        Task RemoveTransactionAsync(Transaction transaction);
    }
}