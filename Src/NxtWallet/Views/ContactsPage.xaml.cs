using NxtWallet.ViewModel;

namespace NxtWallet.Views
{
    public sealed partial class ContactsPage
    {
        private ContactsViewModel ViewModel => (ContactsViewModel) DataContext;

        public ContactsPage()
        {
            InitializeComponent();
        }
    }
}
