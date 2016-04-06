using GalaSoft.MvvmLight;
using NxtWallet.ViewModel.Model;

namespace NxtWallet.ViewModel
{
    public class TransactionDetailViewModel : ViewModelBase
    {
        private TransactionModel _transaction;

        public TransactionModel Transaction
        {
            get { return _transaction; }
            set { Set(ref _transaction, value); }
        }
    }
}
