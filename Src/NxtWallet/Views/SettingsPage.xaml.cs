using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class SettingsPage
    {
        private SettingsViewModel ViewModel => (SettingsViewModel) DataContext;

        public SettingsPage()
        {
            InitializeComponent();
        }
    }
}
