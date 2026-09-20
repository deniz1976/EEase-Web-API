using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserNotFoundException = EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class RouteQueryService : IRouteQueryService
    {
        private const int UserNotFoundCode = 7;

        private readonly UserManager<AppUser> _userManager;
        private readonly EEaseAPIDbContext _context;
        private readonly IRouteAccessPolicy _routeAccessPolicy;
        private readonly IFriendshipService _friendshipService;

        public RouteQueryService(
            UserManager<AppUser> userManager,
            EEaseAPIDbContext context,
            IRouteAccessPolicy routeAccessPolicy,
            IFriendshipService friendshipService)
        {
            _userManager = userManager;
            _context = context;
            _routeAccessPolicy = routeAccessPolicy;
            _friendshipService = friendshipService;
        }

        public async Task<(List<StandardRoute> Routes, int TotalCount)> GetAllRoutes(
            string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var user = await RequireUserAsync(username);

            var query = _context.StandardRoutes
                .AsNoTracking()
                .IncludeFullRouteGraph()
                .Where(route => route.UserId == user.Id)
                .OrderByDescending(route => route.CreatedDate);

            var (routes, totalCount) = await PageAsync(query, pageNumber, pageSize, cancellationToken);

            foreach (var route in routes)
            {
                route.User = null;
                route.LikedUsers = null;
            }

            return (routes, totalCount);
        }

        public async Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetLikedRoutes(
            string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var user = await RequireUserAsync(username);

            var query = _context.StandardRoutes
                .AsNoTracking()
                .IncludeFullRouteGraph()
                .Where(route => route.LikedUsers!.Any(liker => liker.Id == user.Id))
                .OrderByDescending(route => route.CreatedDate);

            var (routes, totalCount) = await PageAsync(query, pageNumber, pageSize, cancellationToken);

            return (await ToDtosAsync(routes, username, user.Id, cancellationToken), totalCount);
        }

        public async Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetRoutesByUserId(
            string userId, string requesterUsername, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var targetUser = await _userManager.FindByIdAsync(userId)
                ?? throw new UserNotFoundException("Target user not found", UserNotFoundCode);

            var requester = await _userManager.FindByNameAsync(requesterUsername)
                ?? throw new UserNotFoundException("Requester not found", UserNotFoundCode);

            var isOwnProfile = targetUser.Id == requester.Id;

            var areFriends = !isOwnProfile
                && await _friendshipService.AreFriendsAsync(targetUser.UserName!, requesterUsername, cancellationToken);

            var query = _context.StandardRoutes
                .AsNoTracking()
                .IncludeFullRouteGraph()
                .Where(route => route.UserId == targetUser.Id);

            if (!isOwnProfile)
            {
                query = query.Where(route =>
                    route.Status == (int)RouteVisibility.Public
                    || (route.Status == (int)RouteVisibility.FriendsOnly && areFriends));
            }

            var (routes, totalCount) = await PageAsync(
                query.OrderByDescending(route => route.CreatedDate), pageNumber, pageSize, cancellationToken);

            return (await ToDtosAsync(routes, requesterUsername, requester.Id, cancellationToken), totalCount);
        }

        public async Task<StandardRouteDTO> GetRouteById(
            string username, Guid? routeId, CancellationToken cancellationToken = default)
        {
            var user = await RequireUserAsync(username);

            var route = await _context.StandardRoutes
                .AsNoTracking()
                .IncludeFullRouteGraph()
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId)
                ?? throw new RouteNotFoundException("Route not found", (int)StatusEnum.RouteNotFound);

            return await _routeAccessPolicy.ToDtoAsync(route, username, user.Id, cancellationToken);
        }

        public async Task<bool> CheckRouteLikeStatus(
            string username, Guid routeId, CancellationToken cancellationToken = default)
        {
            var user = await RequireUserAsync(username);

            var route = await _context.StandardRoutes
                .Include(candidate => candidate.User)
                .Include(candidate => candidate.LikedUsers)
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId)
                ?? throw new RouteNotFoundException("Route not found", (int)StatusEnum.RouteNotFound);

            var access = await _routeAccessPolicy.EvaluateAsync(route, username, user.Id, cancellationToken);

            if (!access.IsAccessible)
                throw new ForbiddenException(access.Message!, StatusEnum.UnauthorizedToViewRoute);

            return route.LikedUsers?.Any(liker => liker.Id == user.Id) == true;
        }

        private async Task<AppUser> RequireUserAsync(string username) =>
            await _userManager.FindByNameAsync(username)
            ?? throw new UserNotFoundException("User not found", UserNotFoundCode);

        private static async Task<(List<StandardRoute> Routes, int TotalCount)> PageAsync(
            IQueryable<StandardRoute> query, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var totalCount = await query.CountAsync(cancellationToken);

            var routes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (routes, totalCount);
        }

        private async Task<List<StandardRouteDTO>> ToDtosAsync(
            IReadOnlyList<StandardRoute> routes,
            string username,
            string userId,
            CancellationToken cancellationToken)
        {
            var dtos = new List<StandardRouteDTO>(routes.Count);

            foreach (var route in routes)
            {
                dtos.Add(await _routeAccessPolicy.ToDtoAsync(route, username, userId, cancellationToken));
            }

            return dtos;
        }
    }
}
