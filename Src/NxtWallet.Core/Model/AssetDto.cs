using System.Collections.Generic;

namespace NxtWallet.Core.Model
{
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
