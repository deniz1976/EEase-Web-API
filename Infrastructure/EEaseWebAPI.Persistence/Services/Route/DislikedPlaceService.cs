using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class DislikedPlaceService : IDislikedPlaceService
    {
        private readonly EEaseAPIDbContext _context;

        public DislikedPlaceService(EEaseAPIDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<string>> GetGoogleIdsAsync(
            string userId, CancellationToken cancellationToken = default) =>
            await _context.UserDislikedPlaces
                .Where(disliked => disliked.UserId == userId)
                .Select(disliked => disliked.GoogleId)
                .ToListAsync(cancellationToken);

        public async Task RecordAsync(
            string userId, string googleId, string placeType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(googleId))
            {
                return;
            }

            var exists = await _context.UserDislikedPlaces
                .AnyAsync(disliked => disliked.UserId == userId && disliked.GoogleId == googleId, cancellationToken);

            if (exists)
            {
                return;
            }

            await _context.UserDislikedPlaces.AddAsync(
                UserDislikedPlace.Create(userId, googleId, placeType), cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
