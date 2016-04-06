using Windows.UI.Xaml.Navigation;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.SelectedContact = null;
        }
    }
}
