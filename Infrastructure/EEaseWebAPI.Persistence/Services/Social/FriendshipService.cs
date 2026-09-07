using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EEaseWebAPI.Persistence.Services.Social
{
    public class FriendshipService : IFriendshipService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EEaseAPIDbContext _context;

        public FriendshipService(UserManager<AppUser> userManager, EEaseAPIDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<UserFriendship?> GetFriendshipAsync(string username, string targetUsername)
        {
            var (user, target) = await ResolveAsync(username, targetUsername, allowSelf: true);

            return await FindFriendshipAsync(user.Id, target.Id);
        }

        public async Task SendRequestAsync(string requesterUsername, string addresseeUsername)
        {
            var (requester, addressee) = await ResolveAsync(requesterUsername, addresseeUsername);

            var blocks = await FindBlocksAsync(requester.Id, addressee.Id);

            if (blocks.Any(block => block.BlockerId == requester.Id))
                throw new UserBlockedException("Unblock this user before sending a friend request.");

            if (blocks.Count > 0)
                throw new UserBlockedException("You cannot send a friend request to this user.");

            var friendship = await FindFriendshipAsync(requester.Id, addressee.Id);

            if (friendship == null)
            {
                await _context.UserFriendships.AddAsync(
                    UserFriendship.Create(requester.Id, addressee.Id, FriendshipStatus.Pending));

                await _context.SaveChangesAsync();
                return;
            }

            if (friendship.Status == FriendshipStatus.Accepted)
                throw new FriendshipException("You are already friends with this user.", StatusEnum.FriendRequestSendFailed);

            if (friendship.Status == FriendshipStatus.Pending)
                throw new FriendRequestAlreadyExistsException();

            friendship.SetParticipants(requester.Id, addressee.Id);
            friendship.Status = FriendshipStatus.Pending;
            friendship.RequestDate = DateTime.UtcNow;
            friendship.ResponseDate = null;

            await _context.SaveChangesAsync();
        }

        public async Task RespondToRequestAsync(string requesterUsername, string addresseeUsername, FriendshipStatus response)
        {
            if (response != FriendshipStatus.Accepted && response != FriendshipStatus.Rejected)
                throw new InvalidFriendshipStatusException("A friend request can only be accepted or rejected.");

            var (requester, addressee) = await ResolveAsync(requesterUsername, addresseeUsername);

            var friendship = await FindFriendshipAsync(requester.Id, addressee.Id)
                ?? throw new FriendshipNotFoundException();

            if (friendship.AddresseeId != addressee.Id)
                throw new FriendshipException(
                    "Only the recipient of the friend request can respond to it.",
                    StatusEnum.FriendRequestResponseFailed);

            if (friendship.Status != FriendshipStatus.Pending)
                throw new FriendshipException(
                    "Can only respond to pending friend requests.",
                    StatusEnum.FriendRequestResponseFailed);

            friendship.Status = response;
            friendship.ResponseDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task CancelRequestAsync(string username, string targetUsername)
        {
            var (user, target) = await ResolveAsync(username, targetUsername);

            var friendship = await FindFriendshipAsync(user.Id, target.Id);

            if (friendship == null ||
                friendship.Status != FriendshipStatus.Pending ||
                friendship.RequesterId != user.Id)
            {
                throw new FriendshipNotFoundException();
            }

            _context.UserFriendships.Remove(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFriendAsync(string username, string friendUsername)
        {
            var (user, friend) = await ResolveAsync(username, friendUsername);

            var friendship = await FindFriendshipAsync(user.Id, friend.Id);

            if (friendship == null || friendship.Status != FriendshipStatus.Accepted)
                throw new FriendshipNotFoundException();

            _context.UserFriendships.Remove(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<UserFriendship>> GetFriendsAsync(string username)
        {
            var user = await FindUserAsync(username);

            return await _context.UserFriendships
                .Where(friendship =>
                    (friendship.UserAId == user.Id || friendship.UserBId == user.Id) &&
                    friendship.Status == FriendshipStatus.Accepted)
                .Include(friendship => friendship.Requester)
                .Include(friendship => friendship.Addressee)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<UserFriendship>> GetPendingRequestsAsync(string username)
        {
            var user = await FindUserAsync(username);

            return await _context.UserFriendships
                .Where(friendship =>
                    friendship.AddresseeId == user.Id &&
                    friendship.Status == FriendshipStatus.Pending)
                .Include(friendship => friendship.Requester)
                .OrderByDescending(friendship => friendship.RequestDate)
                .ToListAsync();
        }

        public async Task<bool> AreFriendsAsync(string username, string targetUsername)
        {
            var user = await _userManager.FindByNameAsync(username);
            var target = await _userManager.FindByNameAsync(targetUsername);

            if (user == null || target == null || user.Id == target.Id)
                return false;

            var pair = UserFriendship.NormalizePair(user.Id, target.Id);

            return await _context.UserFriendships.AnyAsync(friendship =>
                friendship.UserAId == pair.UserAId &&
                friendship.UserBId == pair.UserBId &&
                friendship.Status == FriendshipStatus.Accepted);
        }

        public async Task BlockAsync(string username, string targetUsername)
        {
            var (user, target) = await ResolveAsync(username, targetUsername);

            var alreadyBlocked = await _context.UserBlocks
                .AnyAsync(block => block.BlockerId == user.Id && block.BlockedId == target.Id);

            if (alreadyBlocked)
                throw new UserAlreadyBlockedException();

            var friendship = await FindFriendshipAsync(user.Id, target.Id);

            if (friendship != null)
                _context.UserFriendships.Remove(friendship);

            await _context.UserBlocks.AddAsync(new UserBlock
            {
                BlockerId = user.Id,
                BlockedId = target.Id,
                BlockedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task UnblockAsync(string username, string targetUsername)
        {
            var (user, target) = await ResolveAsync(username, targetUsername);

            var block = await _context.UserBlocks
                .FirstOrDefaultAsync(block => block.BlockerId == user.Id && block.BlockedId == target.Id)
                ?? throw new FriendshipNotFoundException();

            _context.UserBlocks.Remove(block);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<UserBlock>> GetBlockedUsersAsync(string username)
        {
            var user = await FindUserAsync(username);

            return await _context.UserBlocks
                .Where(block => block.BlockerId == user.Id)
                .Include(block => block.Blocked)
                .OrderByDescending(block => block.BlockedDate)
                .ToListAsync();
        }

        public async Task<FriendRequestStatus> GetRequestStatusAsync(string username, string targetUsername)
        {
            await ResolveAsync(username, targetUsername);

            var relationship = await GetRelationshipAsync(username, targetUsername);
            return relationship.RequestStatus;
        }

        public async Task<ProfileVisibilityStatus> GetVisibilityAsync(string username, string targetUsername)
        {
            var relationship = await GetRelationshipAsync(username, targetUsername);
            return relationship.Visibility;
        }

        public async Task<UserRelationship> GetRelationshipAsync(string username, string targetUsername)
        {
            var (user, target) = await ResolveAsync(username, targetUsername, allowSelf: true);

            if (user.Id == target.Id)
                return UserRelationship.Self;

            var blocks = await FindBlocksAsync(user.Id, target.Id);

            if (blocks.Any(block => block.BlockerId == target.Id))
                return new UserRelationship(ProfileVisibilityStatus.BlockedByTarget, FriendRequestStatus.Blocked);

            if (blocks.Count > 0)
                return new UserRelationship(ProfileVisibilityStatus.BlockedTarget, FriendRequestStatus.Blocked);

            var friendship = await FindFriendshipAsync(user.Id, target.Id);

            if (friendship == null)
                return new UserRelationship(ProfileVisibilityStatus.LimitedAccess, FriendRequestStatus.NoRequest);

            if (friendship.Status == FriendshipStatus.Accepted)
                return new UserRelationship(ProfileVisibilityStatus.FullAccess, FriendRequestStatus.AlreadyFriends);

            if (friendship.Status == FriendshipStatus.Pending)
            {
                var requestStatus = friendship.RequesterId == user.Id
                    ? FriendRequestStatus.Requester
                    : FriendRequestStatus.Addressee;

                return new UserRelationship(ProfileVisibilityStatus.LimitedAccess, requestStatus);
            }

            return new UserRelationship(ProfileVisibilityStatus.LimitedAccess, FriendRequestStatus.NoRequest);
        }

        private async Task<(AppUser User, AppUser Target)> ResolveAsync(
            string username, string targetUsername, bool allowSelf = false)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(targetUsername))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            var user = await FindUserAsync(username);
            var target = await FindUserAsync(targetUsername);

            if (!allowSelf && user.Id == target.Id)
                throw new CannotPerformActionOnSelfException();

            return (user, target);
        }

        private async Task<AppUser> FindUserAsync(string username) =>
            await _userManager.FindByNameAsync(username)
            ?? throw new UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

        private Task<UserFriendship?> FindFriendshipAsync(string userId, string otherUserId)
        {
            var pair = UserFriendship.NormalizePair(userId, otherUserId);

            return _context.UserFriendships
                .FirstOrDefaultAsync(friendship =>
                    friendship.UserAId == pair.UserAId &&
                    friendship.UserBId == pair.UserBId);
        }

        private Task<List<UserBlock>> FindBlocksAsync(string userId, string otherUserId) =>
            _context.UserBlocks
                .Where(block =>
                    (block.BlockerId == userId && block.BlockedId == otherUserId) ||
                    (block.BlockerId == otherUserId && block.BlockedId == userId))
                .ToListAsync();
    }
}
