namespace NxtWallet.Core.Models
{
    public class ShufflingCreationTransaction : Transaction
    {
        public int RegistrationPeriod { get; set; }
        public bool Done { get; set; } = false;
    }
}
