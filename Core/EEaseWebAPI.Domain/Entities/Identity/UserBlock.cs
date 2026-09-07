using EEaseWebAPI.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace EEaseWebAPI.Domain.Entities.Identity
{
    public class UserBlock : BaseEntity
    {
        public string BlockerId { get; set; }
        [ForeignKey("BlockerId")]
        public AppUser Blocker { get; set; }

        public string BlockedId { get; set; }
        [ForeignKey("BlockedId")]
        public AppUser Blocked { get; set; }

        public DateTime BlockedDate { get; set; }
    }
}
