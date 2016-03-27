using System;
using Windows.UI.Xaml.Controls;

namespace NxtWallet
{
    public class NavMenuItem
    {
        public string Label { get; set; }
        public Symbol Symbol { get; set; }
        public char SymbolAsChar => (char)Symbol;
        public Type DestPage { get; set; }
        public object Arguments { get; set; }
    }
}
