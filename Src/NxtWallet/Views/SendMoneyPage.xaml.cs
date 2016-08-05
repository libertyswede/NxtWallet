﻿using Windows.UI.Xaml.Navigation;
using NxtWallet.ViewModel;
using NxtWallet.Core.Models;
using Windows.UI.Xaml.Controls;
using System.Linq;
using Windows.UI.Xaml;

namespace NxtWallet.Views
{
    public sealed partial class SendMoneyPage
    {
        private SendMoneyViewModel ViewModel => (SendMoneyViewModel) DataContext;

        public SendMoneyPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.OnNavigatedTo(e.Parameter as Contact);
        }

        private async void RecipientBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var matchingContacts = await ViewModel.GetMatchingRecipients(sender.Text);
                sender.ItemsSource = matchingContacts.ToList();
            }
        }
        
        private async void RecipientBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion == null)
            {
                var matchingContacts = await ViewModel.GetMatchingRecipients(args.QueryText);
                sender.ItemsSource = matchingContacts.ToList();
            }
        }
        
        private void RecipientBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var contact = args.SelectedItem as Contact;
            sender.Text = contact.NxtAddressRs;
        }

        private void RecipientBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RecipientValidation.Visibility = Visibility.Visible;
        }
    }
}
