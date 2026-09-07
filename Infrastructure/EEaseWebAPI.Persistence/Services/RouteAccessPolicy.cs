using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;

namespace EEaseWebAPI.Persistence.Services
{
    public sealed class RouteAccessPolicy : IRouteAccessPolicy
    {
        private readonly IFriendshipService _friendshipService;

        public RouteAccessPolicy(IFriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }

        public async Task<RouteAccessResult> EvaluateAsync(
            StandardRoute route,
            string requesterUsername,
            string requesterUserId)
        {
            ArgumentNullException.ThrowIfNull(route);

            if (route.UserId == requesterUserId)
            {
                return RouteAccessResult.Allowed;
            }

            return (RouteVisibility?)route.Status switch
            {
                RouteVisibility.Private =>
                    RouteAccessResult.Denied("This route is private and only accessible to its owner"),

                RouteVisibility.FriendsOnly =>
                    await IsFriendOfOwnerAsync(route, requesterUsername)
                        ? RouteAccessResult.Allowed
                        : RouteAccessResult.Denied("This route is only accessible to friends"),

                RouteVisibility.Public => RouteAccessResult.Allowed,

                _ => RouteAccessResult.Denied("Invalid route status")
            };
        }

        public async Task<StandardRouteDTO> ToDtoAsync(
            StandardRoute route,
            string requesterUsername,
            string requesterUserId)
        {
            var access = await EvaluateAsync(route, requesterUsername, requesterUserId);

            var dto = new StandardRouteDTO
            {
                Id = route.Id,
                City = route.City,
                Days = route.Days,
                Name = route.Name,
                CreatedDate = route.CreatedDate,
                LikeCount = route.LikeCount,
                UserId = route.UserId,
                IsAccessible = access.IsAccessible,
                AccessibilityMessage = access.Message
            };

            if (access.IsAccessible)
            {
                dto.Status = route.Status;
                dto.TravelDays = route.TravelDays?.ToList();
            }

            return dto;
        }

        private async Task<bool> IsFriendOfOwnerAsync(StandardRoute route, string requesterUsername)
        {
            var ownerUsername = route.User?.UserName;

            if (string.IsNullOrWhiteSpace(ownerUsername))
            {
                return false;
            }

            return await _friendshipService.AreFriendsAsync(ownerUsername, requesterUsername);
        }
    }
}
