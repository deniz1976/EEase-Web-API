using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Enums;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IFriendshipService
    {
        Task<UserFriendship?> GetFriendshipAsync(string username, string targetUsername, CancellationToken cancellationToken = default);

        Task SendRequestAsync(string requesterUsername, string addresseeUsername, CancellationToken cancellationToken = default);

        Task RespondToRequestAsync(string requesterUsername, string addresseeUsername, FriendshipStatus response, CancellationToken cancellationToken = default);

        Task CancelRequestAsync(string username, string targetUsername, CancellationToken cancellationToken = default);

        Task RemoveFriendAsync(string username, string friendUsername, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserFriendship>> GetFriendsAsync(string username, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserFriendship>> GetPendingRequestsAsync(string username, CancellationToken cancellationToken = default);

        Task<bool> AreFriendsAsync(string username, string targetUsername, CancellationToken cancellationToken = default);

        Task BlockAsync(string username, string targetUsername, CancellationToken cancellationToken = default);

        Task UnblockAsync(string username, string targetUsername, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserBlock>> GetBlockedUsersAsync(string username, CancellationToken cancellationToken = default);

        Task<FriendRequestStatus> GetRequestStatusAsync(string username, string targetUsername, CancellationToken cancellationToken = default);

        Task<ProfileVisibilityStatus> GetVisibilityAsync(string username, string targetUsername, CancellationToken cancellationToken = default);

        Task<UserRelationship> GetRelationshipAsync(string username, string targetUsername, CancellationToken cancellationToken = default);
    }
}
