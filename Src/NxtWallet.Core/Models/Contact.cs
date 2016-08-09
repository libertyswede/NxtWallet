using GalaSoft.MvvmLight;
using System;
using System.Text.RegularExpressions;

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

        public static string GetAddressOrInput(string input)
        {
            var match = Regex.Match(input, "\\((.*)\\)$");
            return match.Success ? match.Groups[1].Value : input;
        }

        public override string ToString()
        {
            return $"{Name} ({NxtAddressRs})";
        }
    }
}
