using GalaSoft.MvvmLight;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.ViewModel
{
    public class TransactionDetailViewModel : ViewModelBase
    {
        private Transaction _transaction;
        private string _transactionLink;

        public Transaction Transaction
        {
            get { return _transaction; }
            set
            {
                Set(ref _transaction, value);
                UpdateTransactionLink();
            }
        }

        public string TransactionLink
        {
            get { return _transactionLink; }
            set { Set(ref _transactionLink, value); }
        }

        private void UpdateTransactionLink()
        {
            TransactionLink = $"https://nxtportal.org/transactions/{Transaction.NxtId}";
        }
    }
}
