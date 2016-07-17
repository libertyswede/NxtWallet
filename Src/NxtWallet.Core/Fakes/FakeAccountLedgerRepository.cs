using NxtWallet.Core.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxtWallet.Core.Models;
using System;

namespace NxtWallet.Core.Fakes
{
    public class FakeAccountLedgerRepository : IAccountLedgerRepository
    {
        public Task<List<LedgerEntry>> GetAllLedgerEntriesAsync()
        {
            return Task.FromResult(new List<LedgerEntry>());
        }

        public Task<List<LedgerEntry>> GetUnconfirmedLedgerEntriesAsync()
        {
            return Task.FromResult(new List<LedgerEntry>());
        }

        public Task AddLedgerEntriesAsync(List<LedgerEntry> ledgerEntries)
        {
            return Task.CompletedTask;
        }

        public Task AddLedgerEntryAsync(LedgerEntry ledgerEntry)
        {
            return Task.CompletedTask;
        }

        public Task UpdateLedgerEntriesAsync(List<LedgerEntry> updatedLedgerEntries)
        {
            return Task.CompletedTask;
        }

        public Task<List<LedgerEntry>> GetLedgerEntriesOnLastBlockAsync()
        {
            return Task.FromResult(new List<LedgerEntry>());
        }

        public Task RemoveLedgerEntriesOnBlockAsync(ulong blockId)
        {
            return Task.CompletedTask;
        }
    }
}
