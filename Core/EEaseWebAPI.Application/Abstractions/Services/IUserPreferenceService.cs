using EEaseWebAPI.Application.Features.Queries.AppUser.GetAllTopics;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;
using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserPreferenceService
    {
        Task<AppUser> GetUserWithPreferencesAsync(string username, CancellationToken cancellationToken = default);

        Task SetFromMessageAsync(string username, string message, CancellationToken cancellationToken = default);

        Task SetFromTopicsAsync(string username, IReadOnlyList<string> topics, CancellationToken cancellationToken = default);

        Task ResetAsync(string username, CancellationToken cancellationToken = default);

        Task<GetUserPreferenceDescriptionsBody> GetDescriptionsAsync(string username, CancellationToken cancellationToken = default);

        Task<GetUserPreferenceDescriptionsBody?> GetDescriptionsForViewerAsync(string viewerUsername, string targetUsername, CancellationToken cancellationToken = default);

        GetAllTopicsQueryResponseBody GetAllTopics();
    }
}
