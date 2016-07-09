using NxtWallet.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace NxtWallet.Core.Repositories
{
    public interface IAccountLedgerRepository
    {
        Task<IEnumerable<LedgerEntry>> GetAllLedgerEntriesAsync();
        Task SaveEntryAsync(LedgerEntry ledgerEntry);
    }

    public class AccountLedgerRepository : IAccountLedgerRepository
    {
        public Task<IEnumerable<LedgerEntry>> GetAllLedgerEntriesAsync()
        {
            throw new NotImplementedException();
        }

        public Task SaveEntryAsync(LedgerEntry ledgerEntry)
        {
            throw new NotImplementedException();
        }
    }
}
