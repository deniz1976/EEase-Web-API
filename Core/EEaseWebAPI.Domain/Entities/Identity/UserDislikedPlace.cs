using EEaseWebAPI.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace EEaseWebAPI.Domain.Entities.Identity
{
    public class UserDislikedPlace : BaseEntity
    {
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public AppUser User { get; set; } = null!;

        public string GoogleId { get; set; } = null!;

        public string PlaceType { get; set; } = null!;

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
