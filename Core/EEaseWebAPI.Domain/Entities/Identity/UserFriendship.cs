using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace EEaseWebAPI.Domain.Entities.Identity
{
    public class UserFriendship : BaseEntity
    {
        public string RequesterId { get; set; }
        [ForeignKey("RequesterId")]
        public AppUser Requester { get; set; }

        public string AddresseeId { get; set; }
        [ForeignKey("AddresseeId")]
        public AppUser Addressee { get; set; }

        public FriendshipStatus Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ResponseDate { get; set; }
    }
} 