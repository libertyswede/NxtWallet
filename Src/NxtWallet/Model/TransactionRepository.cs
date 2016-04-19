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
                transactionDtos.ForEach(dto => transactionList.Single(t => t.NxtId == (ulong?)dto.NxtId).Id = dto.Id); // TODO: This will crash if > 1 null value
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