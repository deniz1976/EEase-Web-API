using EEaseWebAPI.Application.Features.Queries.AppUser.GetAllTopics;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;
using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserPreferenceService
    {
        Task<AppUser> GetUserWithPreferencesAsync(string username);

        Task SetFromMessageAsync(string username, string message);

        Task SetFromTopicsAsync(string username, IReadOnlyList<string> topics);

        Task ResetAsync(string username);

        Task<GetUserPreferenceDescriptionsBody> GetDescriptionsAsync(string username);

        Task<GetUserPreferenceDescriptionsBody?> GetDescriptionsForViewerAsync(string viewerUsername, string targetUsername);

        GetAllTopicsQueryResponseBody GetAllTopics();
    }
}
