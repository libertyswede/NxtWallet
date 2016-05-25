namespace NxtWallet.ViewModel.Model
{
    public class ShufflingCreationTransaction : Transaction
    {
        public int RegistrationPeriod { get; set; }
        public bool Done { get; set; } = false;
    }
}
