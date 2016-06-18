using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NxtWallet.Core.Migrations.Model
{
    [Table("Asset")]
    public class AssetDto
    {
        public int Id { get; set; }
        public long NxtId { get; set; }
        public int Decimals { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }

        public IList<AssetOwnershipDto> Ownerships { get; set; }
    }
}
