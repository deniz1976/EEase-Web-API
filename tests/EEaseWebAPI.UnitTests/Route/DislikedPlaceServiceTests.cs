using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class DislikedPlaceServiceTests : IDisposable
    {
        private readonly EEaseAPIDbContext _context;
        private readonly DislikedPlaceService _service;

        public DislikedPlaceServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"disliked-{Guid.NewGuid():N}")
                    .Options);

            _service = new DislikedPlaceService(_context);
        }

        public void Dispose() => _context.Dispose();

        [Fact]
        public async Task A_user_who_disliked_nothing_has_an_empty_list()
        {
            (await _service.GetGoogleIdsAsync("alice-id")).Should().BeEmpty();
        }

        [Fact]
        public async Task A_dislike_is_remembered()
        {
            await _service.RecordAsync("alice-id", "place-1", "dinner");

            (await _service.GetGoogleIdsAsync("alice-id")).Should().Equal("place-1");
        }

        [Fact]
        public async Task Disliking_the_same_place_twice_stores_one_row()
        {
            await _service.RecordAsync("alice-id", "place-1", "dinner");
            await _service.RecordAsync("alice-id", "place-1", "lunch");

            (await _service.GetGoogleIdsAsync("alice-id")).Should().Equal("place-1");
        }

        [Fact]
        public async Task Dislikes_are_private_to_the_user_who_made_them()
        {
            await _service.RecordAsync("alice-id", "place-1", "dinner");

            (await _service.GetGoogleIdsAsync("bob-id")).Should().BeEmpty();
        }

        [Fact]
        public async Task An_empty_google_id_is_not_stored()
        {
            await _service.RecordAsync("alice-id", "  ", "dinner");

            (await _service.GetGoogleIdsAsync("alice-id")).Should().BeEmpty();
        }
    }
}
