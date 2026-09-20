using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Caching;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace EEaseWebAPI.UnitTests.Caching
{
    public class UserCacheServiceTests : IDisposable
    {
        private readonly EEaseAPIDbContext _context;
        private readonly UserCacheService _service;

        private readonly AppUser _alice = Confirmed("alice-id", "alice", "Alice", "Doe");
        private readonly AppUser _bob = Confirmed("bob-id", "bob", "Bob", "Stone");
        private readonly AppUser _unconfirmed = new()
        {
            Id = "ghost-id",
            UserName = "ghost",
            Name = "Ghost",
            Surname = "Rider",
            EmailConfirmed = false
        };

        public UserCacheServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"users-{Guid.NewGuid():N}")
                    .Options);

            _context.Users.AddRange(_alice, _bob, _unconfirmed);
            _context.SaveChanges();

            _service = new UserCacheService(
                new MemoryCache(new MemoryCacheOptions()),
                _context,
                Options.Create(new CacheOptions()));
        }

        public void Dispose() => _context.Dispose();

        private static AppUser Confirmed(string id, string username, string name, string surname) => new()
        {
            Id = id,
            UserName = username,
            Name = name,
            Surname = surname,
            EmailConfirmed = true
        };

        [Fact]
        public async Task A_search_finds_a_user_by_username()
        {
            await _service.LoadUsersToCache();

            var results = await _service.SearchUsersAsync("ali");

            results.Single().Username.Should().Be("alice");
        }

        [Fact]
        public async Task A_search_finds_a_user_by_their_full_name()
        {
            await _service.LoadUsersToCache();

            var results = await _service.SearchUsersAsync("Bob Stone");

            results.Single().Username.Should().Be("bob");
        }

        [Fact]
        public async Task A_search_ignores_case()
        {
            await _service.LoadUsersToCache();

            (await _service.SearchUsersAsync("ALICE")).Should().ContainSingle();
        }

        [Fact]
        public async Task A_user_who_never_confirmed_their_email_is_not_searchable()
        {
            await _service.LoadUsersToCache();

            (await _service.SearchUsersAsync("ghost")).Should().BeEmpty();
        }

        [Fact]
        public async Task An_empty_search_term_finds_nobody()
        {
            await _service.LoadUsersToCache();

            (await _service.SearchUsersAsync("   ")).Should().BeEmpty();
        }

        [Fact]
        public async Task A_search_still_works_when_the_cache_was_never_warmed_up()
        {
            // The entry expires after an hour of no searches; before, that turned every
            // search into an empty result instead of a database read.
            var results = await _service.SearchUsersAsync("alice");

            results.Single().Username.Should().Be("alice");
        }

        [Fact]
        public async Task A_new_user_becomes_searchable_without_reloading_everything()
        {
            await _service.LoadUsersToCache();

            _service.AddOrUpdateUserInCache(Confirmed("carol-id", "carol", "Carol", "Vance"));

            (await _service.SearchUsersAsync("carol")).Should().ContainSingle();
        }

        [Fact]
        public async Task A_user_who_has_not_confirmed_their_email_is_not_added()
        {
            await _service.LoadUsersToCache();

            _service.AddOrUpdateUserInCache(_unconfirmed);

            (await _service.SearchUsersAsync("ghost")).Should().BeEmpty();
        }

        [Fact]
        public async Task A_renamed_user_is_found_under_the_new_name_only()
        {
            await _service.LoadUsersToCache();

            _service.UpdateUserAttributesInCache("alice-id", username: "alicia", name: "Alicia");

            (await _service.SearchUsersAsync("alicia")).Should().ContainSingle();
            (await _service.SearchUsersAsync("alice")).Should().BeEmpty();
        }

        [Fact]
        public async Task Updating_one_attribute_leaves_the_others_alone()
        {
            await _service.LoadUsersToCache();

            _service.UpdateUserAttributesInCache("alice-id", photoUrl: "photo.png");

            var alice = (await _service.SearchUsersAsync("alice")).Single();

            alice.PhotoUrl.Should().Be("photo.png");
            alice.Surname.Should().Be("Doe");
        }

        [Fact]
        public async Task A_deleted_user_disappears_from_search()
        {
            await _service.LoadUsersToCache();

            _service.RemoveUserFromCache("alice-id");

            (await _service.SearchUsersAsync("alice")).Should().BeEmpty();
            (await _service.SearchUsersAsync("bob")).Should().ContainSingle();
        }

        [Fact]
        public async Task Searching_while_the_list_is_being_changed_does_not_fail()
        {
            await _service.LoadUsersToCache();

            for (var index = 0; index < 2_000; index++)
            {
                _service.AddOrUpdateUserInCache(Confirmed($"seed-{index}", $"seed{index}", "Seed", "User"));
            }

            using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            // The list used to be edited in place, so a search reading it at the same time
            // threw "collection was modified" and the request failed with a 500.
            var writes = Task.Run(() =>
            {
                var index = 0;

                while (!stop.IsCancellationRequested)
                {
                    _service.AddOrUpdateUserInCache(
                        Confirmed($"churn-{index % 50}", $"churn{index++ % 50}", "Churn", "User"));
                }
            });

            var searches = Task.Run(async () =>
            {
                while (!stop.IsCancellationRequested)
                {
                    await _service.SearchUsersAsync("zzz-no-match");
                }
            });

            await FluentActions.Awaiting(() => Task.WhenAll(writes, searches))
                .Should().NotThrowAsync();
        }

    }
}
