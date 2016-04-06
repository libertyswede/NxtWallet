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

        public async Task<IEnumerable<TransactionModel>> GetAllTransactionsAsync()
        {
            using (var context = new WalletContext())
            {
                var transactions = await context.Transactions
                    .OrderByDescending(t => t.Timestamp)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<TransactionModel>>(transactions);
            }
        }

        public async Task SaveTransactionAsync(TransactionModel transactionModel)
        {
            var transaction = _mapper.Map<Transaction>(transactionModel);
            using (var context = new WalletContext())
            {
                var existingTransaction = await context.Transactions.SingleOrDefaultAsync(t => t.NxtId == transaction.NxtId);
                if (existingTransaction == null)
                {
                    context.Transactions.Add(transaction);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateTransactionsAsync(IEnumerable<TransactionModel> transactionModels)
        {
            var transactions = _mapper.Map<IEnumerable<Transaction>>(transactionModels);
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

        public async Task SaveTransactionsAsync(IEnumerable<TransactionModel> transactionModels)
        {
            using (var context = new WalletContext())
            {
                var existingTransactions = await GetAllTransactionsAsync();
                foreach (var transaction in transactionModels.Except(existingTransactions))
                {
                    context.Transactions.Add(_mapper.Map<Transaction>(transaction));
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task<TransactionModel> GetLatestTransactionAsync()
        {
            using (var context = new WalletContext())
            {
                var transaction = await context.Transactions
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync();

                return _mapper.Map<TransactionModel>(transaction);
            }
        }
    }
}