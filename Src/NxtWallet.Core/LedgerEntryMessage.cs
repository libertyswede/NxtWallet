using NxtWallet.Core.Models;

namespace NxtWallet.Core
{
    public enum LedgerEntryMessageAction
    {
        Added,
        Removed,
        ConfirmationUpdated
    }

    public class LedgerEntryMessage
    {
        public LedgerEntry LedgerEntry { get; }
        public LedgerEntryMessageAction Action { get; }

        public LedgerEntryMessage(LedgerEntry ledgerEntry, LedgerEntryMessageAction action)
        {
            LedgerEntry = ledgerEntry;
            Action = action;
        }
    }
}
