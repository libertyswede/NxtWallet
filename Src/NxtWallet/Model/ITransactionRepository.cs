using System.Collections.Generic;
using System.Threading.Tasks;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<TransactionModel>> GetAllTransactionsAsync();
        Task SaveTransactionAsync(TransactionModel transactionModel);
        Task UpdateTransactionsAsync(IEnumerable<TransactionModel> transactionModels);
        Task SaveTransactionsAsync(IEnumerable<TransactionModel> transactionModels);
        Task<TransactionModel> GetLatestTransactionAsync();
    }
}