using GalaSoft.MvvmLight;

namespace NxtWallet.ViewModel.Model
{
    public class ContactModel : ObservableObject
    {
        private string _name;
        private string _nxtAddressRs;

        public int Id { get; set; }

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        public string NxtAddressRs
        {
            get { return _nxtAddressRs; }
            set { Set(ref _nxtAddressRs, value); }
        }
    }
}
