using GalaSoft.MvvmLight;
using System;

namespace NxtWallet.Core.Models
{
    public class Contact : ObservableObject, IComparable<Contact>
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

        public int CompareTo(Contact other)
        {
            return Name.CompareTo(other?.Name);
        }

        public override string ToString()
        {
            return Name + " " + NxtAddressRs;
        }
    }
}
