namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IDislikedPlaceService
    {
        Task<IReadOnlyCollection<string>> GetGoogleIdsAsync(string userId, CancellationToken cancellationToken = default);

        Task RecordAsync(string userId, string googleId, string placeType, CancellationToken cancellationToken = default);
    }
}
