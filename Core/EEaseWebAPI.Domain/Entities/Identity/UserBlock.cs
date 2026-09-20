using EEaseWebAPI.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace EEaseWebAPI.Domain.Entities.Identity
{
    public class UserBlock : BaseEntity
    {
        public string BlockerId { get; set; } = null!;
        [ForeignKey("BlockerId")]
        public AppUser Blocker { get; set; } = null!;

        public string BlockedId { get; set; } = null!;
        [ForeignKey("BlockedId")]
        public AppUser Blocked { get; set; } = null!;

        public DateTime BlockedDate { get; set; }
    }
}
