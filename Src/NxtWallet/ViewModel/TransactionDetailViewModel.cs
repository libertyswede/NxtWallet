using GalaSoft.MvvmLight;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.ViewModel
{
    public class TransactionDetailViewModel : ViewModelBase
    {
        private Transaction _transaction;

        public Transaction Transaction
        {
            get { return _transaction; }
            set { Set(ref _transaction, value); }
        }
    }
}
