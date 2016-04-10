using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NxtWallet.Controls
{
    public class SelectableTextBlock : TextBox
    {
        public SelectableTextBlock()
        {
            Background = new SolidColorBrush(Colors.Transparent);
            BorderThickness = new Thickness(0);
            IsReadOnly = true;
            TextWrapping = TextWrapping.Wrap;
        }
    }
}
