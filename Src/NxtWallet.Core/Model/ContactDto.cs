using System.ComponentModel.DataAnnotations.Schema;

namespace NxtWallet.Core.Model
{
    [Table("Contact")]
    public class ContactDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NxtAddressRs { get; set; }
    }
}
