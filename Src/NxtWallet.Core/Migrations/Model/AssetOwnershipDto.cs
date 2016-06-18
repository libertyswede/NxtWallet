using System.ComponentModel.DataAnnotations.Schema;

namespace NxtWallet.Core.Migrations.Model
{
    [Table("AssetOwnership")]
    public class AssetOwnershipDto
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public AssetDto Asset { get; set; }
        public int TransactionId { get; set; }
        public TransactionDto Transaction { get; set; }
        public long QuantityQnt { get; set; }
        public long BalanceQnt { get; set; }
        public int Height { get; set; }
        public int AssetDecimals { get; set; }
    }
}