using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Social;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Friendship
{
    public class FriendshipServiceTests : IDisposable
    {
        private readonly EEaseAPIDbContext _context;
        private readonly FriendshipService _service;

        private readonly AppUser _alice = new() { Id = "alice-id", UserName = "alice" };
        private readonly AppUser _bob = new() { Id = "bob-id", UserName = "bob" };

        public FriendshipServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"friendship-{Guid.NewGuid():N}")
                    .Options);

            var userManager = Substitute.For<UserManager<AppUser>>(
                Substitute.For<IUserStore<AppUser>>(),
                null, null, null, null, null, null, null, null);

            userManager.FindByNameAsync("alice").Returns(_alice);
            userManager.FindByNameAsync("ALICE").Returns(_alice);
            userManager.FindByNameAsync("bob").Returns(_bob);

            _context.Users.AddRange(_alice, _bob);
            _context.SaveChanges();

            _service = new FriendshipService(userManager, _context);
        }

        public void Dispose() => _context.Dispose();

        private async Task<UserFriendship> SeedFriendshipAsync(
            string requesterId,
            string addresseeId,
            FriendshipStatus status)
        {
            var friendship = UserFriendship.Create(requesterId, addresseeId, status);

            await _context.UserFriendships.AddAsync(friendship);
            await _context.SaveChangesAsync();

            return friendship;
        }

        private async Task SeedBlockAsync(string blockerId, string blockedId)
        {
            await _context.UserBlocks.AddAsync(new UserBlock
            {
                BlockerId = blockerId,
                BlockedId = blockedId,
                BlockedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task A_pair_is_stored_in_a_stable_order_whichever_side_asks()
        {
            var first = UserFriendship.Create(_alice.Id, _bob.Id, FriendshipStatus.Pending);
            var second = UserFriendship.Create(_bob.Id, _alice.Id, FriendshipStatus.Pending);

            first.UserAId.Should().Be(second.UserAId);
            first.UserBId.Should().Be(second.UserBId);
            first.RequesterId.Should().Be(_alice.Id);
            second.RequesterId.Should().Be(_bob.Id);

            await Task.CompletedTask;
        }

        [Fact]
        public async Task Unblock_does_not_touch_an_accepted_friendship()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Accepted);

            await Assert.ThrowsAsync<FriendshipNotFoundException>(
                () => _service.UnblockAsync("alice", "bob"));

            _context.UserFriendships.Should().ContainSingle()
                .Which.Status.Should().Be(FriendshipStatus.Accepted);
        }

        [Fact]
        public async Task Unblock_removes_only_a_block_the_caller_owns()
        {
            await SeedBlockAsync(_alice.Id, _bob.Id);

            await _service.UnblockAsync("alice", "bob");

            _context.UserBlocks.Should().BeEmpty();
        }

        [Fact]
        public async Task A_user_cannot_unblock_a_block_that_was_placed_on_them()
        {
            await SeedBlockAsync(_alice.Id, _bob.Id);

            await Assert.ThrowsAsync<FriendshipNotFoundException>(
                () => _service.UnblockAsync("bob", "alice"));

            _context.UserBlocks.Should().ContainSingle();
        }

        [Fact]
        public async Task A_blocked_user_cannot_send_a_friend_request()
        {
            await SeedBlockAsync(_alice.Id, _bob.Id);

            var exception = await Assert.ThrowsAsync<UserBlockedException>(
                () => _service.SendRequestAsync("bob", "alice"));

            exception.Message.Should().Contain("cannot send");
        }

        [Fact]
        public async Task The_blocker_is_told_to_unblock_before_sending_a_request()
        {
            await SeedBlockAsync(_alice.Id, _bob.Id);

            var exception = await Assert.ThrowsAsync<UserBlockedException>(
                () => _service.SendRequestAsync("alice", "bob"));

            exception.Message.Should().Contain("Unblock");
        }

        [Fact]
        public async Task A_rejected_request_can_be_sent_again_and_reuses_the_row()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Rejected);

            await _service.SendRequestAsync("bob", "alice");

            var friendship = await _context.UserFriendships.SingleAsync();
            friendship.Status.Should().Be(FriendshipStatus.Pending);
            friendship.RequesterId.Should().Be(_bob.Id);
            friendship.ResponseDate.Should().BeNull();
        }

        [Fact]
        public async Task A_pending_request_is_not_duplicated_from_the_other_side()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Pending);

            await Assert.ThrowsAsync<FriendRequestAlreadyExistsException>(
                () => _service.SendRequestAsync("bob", "alice"));

            _context.UserFriendships.Should().ContainSingle();
        }

        [Fact]
        public async Task An_existing_friendship_is_not_requested_again()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Accepted);

            await Assert.ThrowsAsync<FriendshipException>(
                () => _service.SendRequestAsync("bob", "alice"));

            _context.UserFriendships.Should().ContainSingle();
        }

        [Fact]
        public async Task A_request_to_yourself_is_rejected_whatever_the_casing()
        {
            await Assert.ThrowsAsync<CannotPerformActionOnSelfException>(
                () => _service.SendRequestAsync("alice", "ALICE"));

            _context.UserFriendships.Should().BeEmpty();
        }

        [Fact]
        public async Task Blocking_drops_the_friendship_and_records_the_block()
        {
            await SeedFriendshipAsync(_bob.Id, _alice.Id, FriendshipStatus.Accepted);

            await _service.BlockAsync("alice", "bob");

            _context.UserFriendships.Should().BeEmpty();

            var block = await _context.UserBlocks.SingleAsync();
            block.BlockerId.Should().Be(_alice.Id);
            block.BlockedId.Should().Be(_bob.Id);
        }

        [Fact]
        public async Task Blocking_drops_the_pending_request_of_the_other_side()
        {
            await SeedFriendshipAsync(_bob.Id, _alice.Id, FriendshipStatus.Pending);

            await _service.BlockAsync("alice", "bob");

            _context.UserFriendships.Should().BeEmpty();
            _context.UserBlocks.Should().ContainSingle();
        }

        [Fact]
        public async Task Blocking_someone_who_already_blocked_you_is_allowed()
        {
            await SeedBlockAsync(_bob.Id, _alice.Id);

            await _service.BlockAsync("alice", "bob");

            var blocks = await _context.UserBlocks.ToListAsync();
            blocks.Should().HaveCount(2);
        }

        [Fact]
        public async Task Unblocking_one_side_leaves_the_other_block_standing()
        {
            await SeedBlockAsync(_bob.Id, _alice.Id);
            await SeedBlockAsync(_alice.Id, _bob.Id);

            await _service.UnblockAsync("alice", "bob");

            var block = await _context.UserBlocks.SingleAsync();
            block.BlockerId.Should().Be(_bob.Id);

            (await _service.GetVisibilityAsync("alice", "bob"))
                .Should().Be(ProfileVisibilityStatus.BlockedByTarget);
        }

        [Fact]
        public async Task Blocking_the_same_user_twice_is_reported()
        {
            await SeedBlockAsync(_alice.Id, _bob.Id);

            await Assert.ThrowsAsync<UserAlreadyBlockedException>(
                () => _service.BlockAsync("alice", "bob"));

            _context.UserBlocks.Should().ContainSingle();
        }

        [Fact]
        public async Task A_block_does_not_erase_the_friendship_of_an_unrelated_pair()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Accepted);
            await SeedBlockAsync(_bob.Id, _alice.Id);

            (await _service.AreFriendsAsync("alice", "bob")).Should().BeTrue();
            (await _service.GetRequestStatusAsync("alice", "bob")).Should().Be(FriendRequestStatus.Blocked);
        }

        [Fact]
        public async Task Only_accepted_and_rejected_are_valid_responses_to_a_request()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Pending);

            await Assert.ThrowsAsync<InvalidFriendshipStatusException>(
                () => _service.RespondToRequestAsync("alice", "bob", (FriendshipStatus)3));

            await Assert.ThrowsAsync<InvalidFriendshipStatusException>(
                () => _service.RespondToRequestAsync("alice", "bob", FriendshipStatus.Pending));

            (await _context.UserFriendships.SingleAsync()).Status.Should().Be(FriendshipStatus.Pending);
        }

        [Fact]
        public async Task Only_the_addressee_can_respond_to_a_request()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Pending);

            await Assert.ThrowsAsync<FriendshipException>(
                () => _service.RespondToRequestAsync("bob", "alice", FriendshipStatus.Accepted));

            (await _context.UserFriendships.SingleAsync()).Status.Should().Be(FriendshipStatus.Pending);
        }

        [Fact]
        public async Task Only_a_pending_request_can_be_answered()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Rejected);

            await Assert.ThrowsAsync<FriendshipException>(
                () => _service.RespondToRequestAsync("alice", "bob", FriendshipStatus.Accepted));

            (await _context.UserFriendships.SingleAsync()).Status.Should().Be(FriendshipStatus.Rejected);
        }

        [Fact]
        public async Task Accepting_a_request_stamps_the_response_date()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Pending);

            await _service.RespondToRequestAsync("alice", "bob", FriendshipStatus.Accepted);

            var friendship = await _context.UserFriendships.SingleAsync();
            friendship.Status.Should().Be(FriendshipStatus.Accepted);
            friendship.ResponseDate.Should().NotBeNull();
        }

        [Fact]
        public async Task Removing_a_friend_only_works_on_an_accepted_friendship()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Pending);

            await Assert.ThrowsAsync<FriendshipNotFoundException>(
                () => _service.RemoveFriendAsync("bob", "alice"));

            _context.UserFriendships.Should().ContainSingle();
        }

        [Fact]
        public async Task Removing_an_accepted_friend_deletes_the_row_from_either_side()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Accepted);

            await _service.RemoveFriendAsync("bob", "alice");

            _context.UserFriendships.Should().BeEmpty();
        }

        [Fact]
        public async Task Cancelling_a_request_you_did_not_send_is_rejected()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Pending);

            await Assert.ThrowsAsync<FriendshipNotFoundException>(
                () => _service.CancelRequestAsync("bob", "alice"));

            _context.UserFriendships.Should().ContainSingle();
        }

        [Fact]
        public async Task Cancelling_your_own_request_removes_it()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Pending);

            await _service.CancelRequestAsync("alice", "bob");

            _context.UserFriendships.Should().BeEmpty();
        }

        [Fact]
        public async Task Checking_your_own_request_status_is_rejected()
        {
            await Assert.ThrowsAsync<CannotPerformActionOnSelfException>(
                () => _service.GetRequestStatusAsync("alice", "ALICE"));
        }

        [Fact]
        public async Task Looking_at_your_own_profile_is_full_access_without_a_request()
        {
            var relationship = await _service.GetRelationshipAsync("alice", "ALICE");

            relationship.Visibility.Should().Be(ProfileVisibilityStatus.FullAccess);
            relationship.RequestStatus.Should().Be(FriendRequestStatus.NoRequest);
        }

        [Fact]
        public async Task The_pending_side_is_reported_from_each_point_of_view()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Pending);

            (await _service.GetRequestStatusAsync("alice", "bob")).Should().Be(FriendRequestStatus.Requester);
            (await _service.GetRequestStatusAsync("bob", "alice")).Should().Be(FriendRequestStatus.Addressee);
        }

        [Fact]
        public async Task Friends_see_each_other_in_full()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Accepted);

            var relationship = await _service.GetRelationshipAsync("bob", "alice");

            relationship.Visibility.Should().Be(ProfileVisibilityStatus.FullAccess);
            relationship.RequestStatus.Should().Be(FriendRequestStatus.AlreadyFriends);
        }

        [Fact]
        public async Task A_stranger_only_gets_limited_access()
        {
            var relationship = await _service.GetRelationshipAsync("bob", "alice");

            relationship.Visibility.Should().Be(ProfileVisibilityStatus.LimitedAccess);
            relationship.RequestStatus.Should().Be(FriendRequestStatus.NoRequest);
        }

        [Fact]
        public async Task Friends_are_listed_from_the_pair_columns()
        {
            await SeedFriendshipAsync(_bob.Id, _alice.Id, FriendshipStatus.Accepted);
            await SeedFriendshipAsync(_alice.Id, "carol-id", FriendshipStatus.Pending);

            var friends = await _service.GetFriendsAsync("alice");

            friends.Should().ContainSingle()
                .Which.Status.Should().Be(FriendshipStatus.Accepted);
        }

        [Fact]
        public async Task Only_incoming_pending_requests_are_listed()
        {
            await SeedFriendshipAsync(_alice.Id, _bob.Id, FriendshipStatus.Pending);

            (await _service.GetPendingRequestsAsync("bob")).Should().ContainSingle();
            (await _service.GetPendingRequestsAsync("alice")).Should().BeEmpty();
        }

        [Fact]
        public async Task Blocked_users_are_listed_with_the_date_they_were_blocked()
        {
            await SeedBlockAsync(_alice.Id, _bob.Id);

            var blocked = (await _service.GetBlockedUsersAsync("alice")).ToList();

            blocked.Should().ContainSingle();
            blocked[0].BlockedId.Should().Be(_bob.Id);
            blocked[0].BlockedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

            (await _service.GetBlockedUsersAsync("bob")).Should().BeEmpty();
        }
    }
}
