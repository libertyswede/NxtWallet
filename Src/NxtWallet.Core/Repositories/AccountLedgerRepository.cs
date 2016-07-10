using NxtWallet.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using NxtWallet.Repositories.Model;
using System.Linq;
using Microsoft.Data.Entity;
using AutoMapper;

namespace NxtWallet.Core.Repositories
{
    public interface IAccountLedgerRepository
    {
        Task<IEnumerable<LedgerEntry>> GetAllLedgerEntriesAsync();
        Task SaveEntryAsync(LedgerEntry ledgerEntry);
    }

    public class AccountLedgerRepository : IAccountLedgerRepository
    {
        private readonly IMapper _mapper;
        private readonly IWalletRepository _walletRepository;

        public AccountLedgerRepository(IMapper mapper, IWalletRepository walletRepository)
        {
            _mapper = mapper;
            _walletRepository = walletRepository;
        }

        public async Task<IEnumerable<LedgerEntry>> GetAllLedgerEntriesAsync()
        {
            using (var context = new WalletContext())
            {
                var ledgerEntryDtos = await context.LedgerEntries
                    .OrderByDescending(entry => entry.Timestamp)
                    .ToListAsync();

                var ledgerEntries = _mapper.Map<List<LedgerEntry>>(ledgerEntryDtos);
                UpdateIsMyAddress(ledgerEntries);
                return ledgerEntries.AsEnumerable();
            }
        }

        public Task SaveEntryAsync(LedgerEntry ledgerEntry)
        {
            throw new NotImplementedException();
        }

        private void UpdateIsMyAddress(List<LedgerEntry> ledgerEntries)
        {
            ledgerEntries.ForEach(t => t.UserIsTransactionRecipient = _walletRepository.NxtAccount.AccountRs == t.AccountTo);
            ledgerEntries.ForEach(t => t.UserIsTransactionSender = _walletRepository.NxtAccount.AccountRs == t.AccountFrom);
        }
    }
}
