namespace NxtWallet.Core.ViewModel.Model
{
    public class Asset
    {
        public int Id { get; set; }
        public long NxtId { get; set; }
        public int Decimals { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
    }
}
