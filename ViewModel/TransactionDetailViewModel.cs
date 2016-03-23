using GalaSoft.MvvmLight;

namespace NxtWallet.ViewModel
{
    public class TransactionDetailViewModel : ViewModelBase
    {
        private ViewModelTransaction _transaction;

        public ViewModelTransaction Transaction
        {
            get { return _transaction; }
            set { Set(ref _transaction, value); }
        }
    }
}
