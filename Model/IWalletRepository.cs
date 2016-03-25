using System.Collections.Generic;
using System.Threading.Tasks;
using NxtLib;

namespace NxtWallet.Model
{
    public interface IWalletRepository
    {
        AccountWithPublicKey NxtAccount { get; }
        string NxtServer { get; }
        string SecretPhrase { get; }
        string Balance { get; }

        Task LoadAsync();
        Task<IEnumerable<ITransaction>> GetAllTransactionsAsync();
        Task SaveTransactionAsync(ITransaction transaction);
        Task UpdateTransactionAsync(ITransaction transaction);
        Task SaveTransactionsAsync(IEnumerable<ITransaction> transactions);
        Task SaveBalanceAsync(string balance);
    }
}