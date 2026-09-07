using EEaseWebAPI.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace EEaseWebAPI.Domain.Entities.Identity
{
    public class UserDislikedPlace : BaseEntity
    {
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        public string GoogleId { get; set; }

        public string PlaceType { get; set; }

        public DateTime DislikedDate { get; set; }

        public static UserDislikedPlace Create(string userId, string googleId, string placeType) =>
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GoogleId = googleId,
                PlaceType = placeType,
                DislikedDate = DateTime.UtcNow
            };
    }
}
