using NxtWallet.Core.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using NxtWallet.Core.Models;
using System.Linq;

namespace NxtWallet.Core.Fakes
{
    public class FakeAccountLedgerRepository : IAccountLedgerRepository
    {
        public Task<IEnumerable<LedgerEntry>> GetAllLedgerEntriesAsync()
        {
            return Task.FromResult(new List<LedgerEntry>().AsEnumerable());
        }

        public Task SaveEntryAsync(LedgerEntry ledgerEntry)
        {
            return Task.CompletedTask;
        }
    }
}
