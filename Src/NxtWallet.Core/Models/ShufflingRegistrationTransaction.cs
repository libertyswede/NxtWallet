namespace NxtWallet.Core.Models
{
    public class ShufflingRegistrationTransaction : Transaction
    {
        public long ShufflingId { get; set; }
        public bool Done { get; set; } = false;
    }
}
