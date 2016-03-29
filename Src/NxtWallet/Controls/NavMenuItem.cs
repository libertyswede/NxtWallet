using System;
using Windows.UI.Xaml;

namespace NxtWallet.Controls
{
    public class NavMenuItem
    {
        public string Label { get; set; }
        public string ImageSource { get; set; }
        public Type DestPage { get; set; }
        public int Width { get; set; } = 20;
        public int Height { get; set; } = 20;
        public object Arguments { get; set; }
    }
}
