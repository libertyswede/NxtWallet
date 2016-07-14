using NxtWallet.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using NxtWallet.Repositories.Model;
using System.Linq;
using Microsoft.Data.Entity;
using AutoMapper;
using NxtWallet.Core.Migrations.Model;

namespace NxtWallet.Core.Repositories
{
    public interface IAccountLedgerRepository
    {
        Task<IEnumerable<LedgerEntry>> GetAllLedgerEntriesAsync();
        Task SaveLedgerEntryAsync(LedgerEntry ledgerEntry);
        Task AddLedgerEntriesAsync(List<LedgerEntry> ledgerEntries);
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

        public async Task AddLedgerEntriesAsync(List<LedgerEntry> ledgerEntries)
        {
            using (var context = new WalletContext())
            {
                var dtos = _mapper.Map<List<LedgerEntryDto>>(ledgerEntries);
                context.LedgerEntries.AddRange(dtos);
                await context.SaveChangesAsync();

                for (int i = 0; i < dtos.Count; i++)
                {
                    ledgerEntries[i].Id = dtos[i].Id;
                }
            }
        }

        public Task SaveLedgerEntryAsync(LedgerEntry ledgerEntry)
        {
            throw new NotImplementedException();
        }

        private void UpdateIsMyAddress(List<LedgerEntry> ledgerEntries)
        {
            ledgerEntries.ForEach(t => t.UserIsRecipient = _walletRepository.NxtAccount.AccountRs == t.AccountTo);
            ledgerEntries.ForEach(t => t.UserIsSender = _walletRepository.NxtAccount.AccountRs == t.AccountFrom);
        }
    }
}
