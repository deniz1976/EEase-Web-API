using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace EEaseWebAPI.Domain.Entities.Identity
{
    public class UserFriendship : BaseEntity
    {
        public string RequesterId { get; set; } = null!;
        [ForeignKey("RequesterId")]
        public AppUser Requester { get; set; } = null!;

        public string AddresseeId { get; set; } = null!;
        [ForeignKey("AddresseeId")]
        public AppUser Addressee { get; set; } = null!;

        public string UserAId { get; set; } = null!;

        public string UserBId { get; set; } = null!;

        public FriendshipStatus Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ResponseDate { get; set; }

        public static UserFriendship Create(string requesterId, string addresseeId, FriendshipStatus status)
        {
            var friendship = new UserFriendship
            {
                Status = status,
                RequestDate = DateTime.UtcNow
            };

            friendship.SetParticipants(requesterId, addresseeId);

            return friendship;
        }

        public void SetParticipants(string requesterId, string addresseeId)
        {
            RequesterId = requesterId;
            AddresseeId = addresseeId;

            var requesterIsFirst = string.CompareOrdinal(requesterId, addresseeId) <= 0;

            UserAId = requesterIsFirst ? requesterId : addresseeId;
            UserBId = requesterIsFirst ? addresseeId : requesterId;
        }

        public static (string UserAId, string UserBId) NormalizePair(string userId, string otherUserId) =>
            string.CompareOrdinal(userId, otherUserId) <= 0
                ? (userId, otherUserId)
                : (otherUserId, userId);
    }
}
