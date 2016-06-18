using NxtWallet.Core;
using System.Threading;
using System.Threading.Tasks;

namespace NxtWallet
{
    public class FakeBackgroundRunner : IBackgroundRunner
    {
        public Task Run(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public event TransactionHandler TransactionConfirmationUpdated;
        public event TransactionHandler TransactionBalanceUpdated;
        public event TransactionHandler TransactionAdded;
        public event BalanceHandler BalanceUpdated;
    }
}