using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Entity;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.Model
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IMapper _mapper;

        public TransactionRepository(IMapper mapper)
        {
            _mapper = mapper;
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
                var existingTransaction = await context.Transactions.SingleOrDefaultAsync(t => t.NxtId == transactionDto.NxtId);
                if (existingTransaction == null)
                {
                    context.Transactions.Add(transactionDto);
                    await context.SaveChangesAsync();
                }
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

        public async Task SaveTransactionsAsync(IEnumerable<Transaction> transactionModels)
        {
            using (var context = new WalletContext())
            {
                var existingTransactions = await GetAllTransactionsAsync();
                foreach (var transaction in transactionModels.Except(existingTransactions))
                {
                    context.Transactions.Add(_mapper.Map<TransactionDto>(transaction));
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task<Transaction> GetLatestTransactionAsync()
        {
            using (var context = new WalletContext())
            {
                var transaction = await context.Transactions
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync();

                return _mapper.Map<Transaction>(transaction);
            }
        }
    }
}