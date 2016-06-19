using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Entity;
using NxtWallet.Core.Models;
using NxtWallet.Core.Migrations.Model;

namespace NxtWallet.Repositories.Model
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

    public class TransactionRepository : ITransactionRepository
    {
        private readonly IMapper _mapper;
        private readonly IWalletRepository _walletRepository;

        public TransactionRepository(IMapper mapper, IWalletRepository walletRepository)
        {
            _mapper = mapper;
            _walletRepository = walletRepository;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            using (var context = new WalletContext())
            {
                var transactions = await context.Transactions
                    .OrderByDescending(t => t.Timestamp)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<Transaction>>(transactions);
            }
        }

        public async Task SaveTransactionAsync(Transaction transaction)
        {
            var transactionDto = _mapper.Map<TransactionDto>(transaction);
            using (var context = new WalletContext())
            {
                context.Transactions.Add(transactionDto);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateTransactionsAsync(IEnumerable<Transaction> transactionModels)
        {
            var transactions = _mapper.Map<IEnumerable<TransactionDto>>(transactionModels);
            using (var context = new WalletContext())
            {
                foreach (var transaction in transactions)
                {
                    context.Transactions.Attach(transaction);
                    context.Entry(transaction).State = EntityState.Modified;
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveTransactionAsync(Transaction transaction)
        {
            using (var context = new WalletContext())
            {
                var transactionDto = _mapper.Map<TransactionDto>(transaction);
                context.Transactions.Attach(transactionDto);
                context.Entry(transactionDto).State = EntityState.Deleted;
                await context.SaveChangesAsync();
            }
        }

        public async Task SaveTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            var transactionList = transactions.ToList();
            using (var context = new WalletContext())
            {
                var transactionDtos = transactionList.Select(t => _mapper.Map<TransactionDto>(t)).ToList();
                foreach (var transaction in transactionDtos)
                {
                    context.Transactions.Add(transaction);
                }
                await context.SaveChangesAsync();
                var savedTransactions = _mapper.Map<IEnumerable<Transaction>>(transactionDtos).ToList();

                savedTransactions.ForEach(saved => transactionList.Find(t => t.Equals(saved)).Id = saved.Id);
            }
        }

        public async Task<bool> HasOutgoingTransactionAsync()
        {
            using (var context = new WalletContext())
            {
                var outgouingTransaction = await context.Transactions
                    .AnyAsync(t => t.AccountFrom == _walletRepository.NxtAccount.AccountRs);
                return outgouingTransaction;
            }
        }
    }
}