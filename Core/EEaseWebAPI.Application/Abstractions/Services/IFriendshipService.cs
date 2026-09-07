using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Enums;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IFriendshipService
    {
        Task<UserFriendship?> GetFriendshipAsync(string username, string targetUsername);

        Task SendRequestAsync(string requesterUsername, string addresseeUsername);

        Task RespondToRequestAsync(string requesterUsername, string addresseeUsername, FriendshipStatus response);

        Task CancelRequestAsync(string username, string targetUsername);

        Task RemoveFriendAsync(string username, string friendUsername);

        Task<IReadOnlyList<UserFriendship>> GetFriendsAsync(string username);

        Task<IReadOnlyList<UserFriendship>> GetPendingRequestsAsync(string username);

        Task<bool> AreFriendsAsync(string username, string targetUsername);

        Task BlockAsync(string username, string targetUsername);

        Task UnblockAsync(string username, string targetUsername);

        Task<IReadOnlyList<UserBlock>> GetBlockedUsersAsync(string username);

        Task<FriendRequestStatus> GetRequestStatusAsync(string username, string targetUsername);

        Task<ProfileVisibilityStatus> GetVisibilityAsync(string username, string targetUsername);

        Task<UserRelationship> GetRelationshipAsync(string username, string targetUsername);
    }
}
