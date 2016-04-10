using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class BackupSecretPhrasePage
    {
        private BackupSecretPhraseViewModel ViewModel => (BackupSecretPhraseViewModel) DataContext;

        public BackupSecretPhrasePage()
        {
            InitializeComponent();
        }
    }
}
