using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.DTOs.Route
{
    public class StandardRouteDTO
    {
        public Guid Id { get; set; }
        public string City { get; set; }
        public int? Days { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? LikeCount { get; set; }
        public string UserId { get; set; }
        public List<TravelDay> TravelDays { get; set; }
        public bool IsAccessible { get; set; } = true;
        public string AccessibilityMessage { get; set; }
        public int? Status { get; set; }
    }
} 