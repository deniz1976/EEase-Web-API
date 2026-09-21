using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Features.Queries.AppUser.CheckFriendRequest;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetPendingFriendRequests;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserFriends;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.Resources;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Friendship
{
    public class FriendshipHandlerTests
    {
        private readonly IFriendshipService _friendship = Substitute.For<IFriendshipService>();
        private readonly IHeaderService _headers = Substitute.For<IHeaderService>();

        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly AppUser _alice = new()
        {
            Id = "alice-id", UserName = "alice", Name = "Alice", Surname = "Doe"
        };

        private readonly AppUser _bob = new()
        {
            Id = "bob-id", UserName = "bob", Name = "Bob", Surname = "Stone"
        };

        public FriendshipHandlerTests()
        {
            _headers.HeaderCreate(Arg.Any<int>()).Returns(call => new Header
            {
                Success = true,
                EnumStatusCode = call.Arg<int>()
            });

            _userManager.FindByNameAsync("alice").Returns(_alice);
        }

        private UserFriendship Accepted(AppUser requester, AppUser addressee)
        {
            var friendship = UserFriendship.Create(requester.Id, addressee.Id, FriendshipStatus.Accepted);

            friendship.Requester = requester;
            friendship.Addressee = addressee;
            friendship.Status = FriendshipStatus.Accepted;
            friendship.ResponseDate = DateTime.UtcNow;

            return friendship;
        }

        [Fact]
        public async Task The_friend_is_whichever_of_the_two_is_not_the_caller()
        {
            _friendship.GetFriendsAsync("alice").Returns(new[]
            {
                Accepted(_alice, _bob),
                Accepted(new AppUser { Id = "carol-id", UserName = "carol", Name = "Carol" }, _alice)
            });

            var handler = new GetUserFriendsQueryHandler(_friendship, _headers, _userManager);

            var response = await handler.Handle(
                new GetUserFriendsQuery { Username = "alice" }, CancellationToken.None);

            response.Body!.Friends.Select(friend => friend.Username)
                .Should().BeEquivalentTo(new[] { "bob", "carol" });
        }

        [Fact]
        public async Task A_friendship_with_no_answer_yet_is_dated_by_the_request()
        {
            var friendship = Accepted(_alice, _bob);
            friendship.ResponseDate = null;
            friendship.RequestDate = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

            _friendship.GetFriendsAsync("alice").Returns(new[] { friendship });

            var handler = new GetUserFriendsQueryHandler(_friendship, _headers, _userManager);

            var response = await handler.Handle(
                new GetUserFriendsQuery { Username = "alice" }, CancellationToken.None);

            response.Body!.Friends.Single().FriendshipDate.Should().Be(friendship.RequestDate);
        }

        [Fact]
        public async Task A_pending_request_is_reported_with_the_person_who_asked()
        {
            var pending = UserFriendship.Create(_bob.Id, _alice.Id, FriendshipStatus.Pending);
            pending.Requester = _bob;
            pending.Addressee = _alice;

            _friendship.GetPendingRequestsAsync("alice").Returns(new[] { pending });

            var handler = new GetPendingFriendRequestsQueryHandler(_friendship, _headers);

            var response = await handler.Handle(
                new GetPendingFriendRequestsQuery { Username = "alice" }, CancellationToken.None);

            var request = response.Body!.PendingRequests.Single();

            request.RequesterUsername.Should().Be("bob");
            request.RequesterName.Should().Be("Bob");
            request.RequesterSurname.Should().Be("Stone");
        }

        [Theory]
        [InlineData(FriendRequestStatus.NoRequest)]
        [InlineData(FriendRequestStatus.Requester)]
        [InlineData(FriendRequestStatus.Addressee)]
        [InlineData(FriendRequestStatus.AlreadyFriends)]
        [InlineData(FriendRequestStatus.Blocked)]
        [InlineData(FriendRequestStatus.Self)]
        public async Task Every_request_status_is_explained_in_words(FriendRequestStatus status)
        {
            _friendship.GetRequestStatusAsync("alice", "bob").Returns(status);

            var handler = new CheckFriendRequestQueryHandler(_friendship, _headers);

            var response = await handler.Handle(
                new CheckFriendRequestQueryRequest { Username = "alice", TargetUsername = "bob" },
                CancellationToken.None);

            response.Body!.Status.Should().Be(status);
            response.Body.Message.Should().NotBeNullOrWhiteSpace()
                .And.NotBe(AppMessages.UnknownFriendRequestStatus);
        }
    }
}
