using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace NxtWallet
{
    public interface IDialog
    {
        void Hide();
        IAsyncOperation<ContentDialogResult> ShowAsync();
    }
}