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
        Task<List<LedgerEntry>> GetAllLedgerEntriesAsync();
        Task<List<LedgerEntry>> GetUnconfirmedLedgerEntriesAsync();
        Task<List<LedgerEntry>> GetLedgerEntriesOnLastBlockAsync();
        Task AddLedgerEntryAsync(LedgerEntry ledgerEntry);
        Task AddLedgerEntriesAsync(List<LedgerEntry> ledgerEntries);
        Task UpdateLedgerEntriesAsync(List<LedgerEntry> updatedLedgerEntries);
        Task DeleteAllLedgerEntriesAsync();
        Task RemoveLedgerEntriesOnBlockAsync(ulong blockId);
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

        public async Task<List<LedgerEntry>> GetUnconfirmedLedgerEntriesAsync()
        {
            using (var context = new WalletContext())
            {
                var ledgerEntryDtos = await context.LedgerEntries
                    .Where(e => e.IsConfirmed == false)
                    .OrderByDescending(entry => entry.Timestamp)
                    .ToListAsync();

                var ledgerEntries = _mapper.Map<List<LedgerEntry>>(ledgerEntryDtos);
                UpdateIsMyAddress(ledgerEntries);
                return ledgerEntries;
            }
        }

        public async Task<List<LedgerEntry>> GetAllLedgerEntriesAsync()
        {
            using (var context = new WalletContext())
            {
                var ledgerEntryDtos = await context.LedgerEntries
                    .OrderByDescending(entry => entry.Timestamp)
                    .ToListAsync();

                var ledgerEntries = _mapper.Map<List<LedgerEntry>>(ledgerEntryDtos);
                UpdateIsMyAddress(ledgerEntries);
                return ledgerEntries;
            }
        }

        public async Task<List<LedgerEntry>> GetLedgerEntriesOnLastBlockAsync()
        {
            using (var context = new WalletContext())
            {
                var ledgerEntryDtos = await context.LedgerEntries
                    .Where(e => e.Height == context.LedgerEntries.Where(e2 => e2.Height != null).Max(e2 => e2.Height))
                    .ToListAsync();

                var ledgerEntries = _mapper.Map<List<LedgerEntry>>(ledgerEntryDtos);
                UpdateIsMyAddress(ledgerEntries);
                return ledgerEntries;
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

        public async Task AddLedgerEntryAsync(LedgerEntry ledgerEntry)
        {
            using (var context = new WalletContext())
            {
                var dto = _mapper.Map<LedgerEntryDto>(ledgerEntry);
                context.LedgerEntries.Add(dto);
                await context.SaveChangesAsync();
                ledgerEntry.Id = dto.Id;
            }
        }

        public async Task UpdateLedgerEntriesAsync(List<LedgerEntry> updatedLedgerEntries)
        {
            using (var context = new WalletContext())
            {
                var dtos = _mapper.Map<List<LedgerEntryDto>>(updatedLedgerEntries);
                foreach (var dto in dtos)
                {
                    context.LedgerEntries.Add(dto);
                    context.Entry(dto).State = EntityState.Modified;
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveLedgerEntriesOnBlockAsync(ulong blockId)
        {
            using (var context = new WalletContext())
            {
                var entries = await context.LedgerEntries.Where(e => e.BlockId == (long)blockId).ToListAsync();
                entries.ForEach(e => context.Entry(e).State = EntityState.Deleted);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllLedgerEntriesAsync()
        {
            using (var context = new WalletContext())
            {
                await context.Database.ExecuteSqlCommandAsync($"DELETE FROM {LedgerEntryDto.TableName}");
            }
        }

        private void UpdateIsMyAddress(List<LedgerEntry> ledgerEntries)
        {
            ledgerEntries.ForEach(t => t.UserIsRecipient = _walletRepository.NxtAccount.AccountRs == t.AccountTo);
            ledgerEntries.ForEach(t => t.UserIsSender = _walletRepository.NxtAccount.AccountRs == t.AccountFrom);
        }
    }
}
