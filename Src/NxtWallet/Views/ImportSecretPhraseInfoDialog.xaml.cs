﻿using GalaSoft.MvvmLight.Messaging;
using NxtWallet.ViewModel;
using Windows.UI.Xaml.Controls;

namespace NxtWallet.Views
{
    public sealed partial class ImportSecretPhraseInfoDialog : ContentDialog
    {
        bool _showing;

        public ImportSecretPhraseInfoDialog()
        {
            Messenger.Default.Register<ImportSecretPhraseMessage>(this, (message) => DoShow(message));
            InitializeComponent();
        }

        private void DoShow(ImportSecretPhraseMessage message)
        {
            if (message.MessageState == ImportSecretPhraseMessage.State.ShowInfo && _showing == false)
            {
                _showing = true;
                var ignore = ShowAsync();
            }
        }

        private void ContinueButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _showing = false;
            Hide();
            Messenger.Default.Send(new ImportSecretPhraseMessage(ImportSecretPhraseMessage.State.Import));
        }

        private void CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _showing = false;
            Hide();
        }
    }
}