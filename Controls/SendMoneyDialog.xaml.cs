namespace NxtWallet.Controls
{
    public interface ISendMoneyDialog : IDialog
    {
    }

    public sealed partial class SendMoneyDialog : ISendMoneyDialog
    {
        public SendMoneyDialog()
        {
            InitializeComponent();
        }
    }
}
