using GalaSoft.MvvmLight;
using NxtWallet.Core.Models;

namespace NxtWallet.ViewModel
{
    public class LedgerEntryDetailViewModel : ViewModelBase
    {
        private LedgerEntry _ledgerEntry;
        private string _transactionLink;

        public LedgerEntry LedgerEntry
        {
            get { return _ledgerEntry; }
            set
            {
                Set(ref _ledgerEntry, value);
                UpdateTransactionLink();
            }
        }

        public string LedgerEntryLink
        {
            get { return _transactionLink; }
            set { Set(ref _transactionLink, value); }
        }

        private void UpdateTransactionLink()
        {
            LedgerEntryLink = $"https://nxtportal.org/transactions/{LedgerEntry.NxtId}";
        }
    }
}
