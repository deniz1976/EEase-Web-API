using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Caching;
using EEaseWebAPI.Persistence.Services.ReferenceData;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace EEaseWebAPI.UnitTests.ReferenceData
{
    // The currency table is keyless, and EF's in-memory provider refuses to track a type
    // without a key, so there is no way to put rows in front of this service here. What can
    // be checked is that it reads the table once and answers out of the cache afterwards.
    public class CurrencyServiceTests : IDisposable
    {
        private readonly EEaseAPIDbContext _context;
        private readonly CurrencyService _service;

        public CurrencyServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"currencies-{Guid.NewGuid():N}")
                    .Options);

            _service = new CurrencyService(
                _context,
                new ReferenceDataCache(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheOptions())));
        }

        public void Dispose() => _context.Dispose();

        [Fact]
        public async Task An_empty_table_is_an_empty_answer_rather_than_a_failure()
        {
            (await _service.GetCurrenciesAsync()).Should().BeEmpty();
        }

        [Fact]
        public async Task The_table_is_read_once_and_then_remembered()
        {
            await _service.GetCurrenciesAsync();

            // A second read would go through a context that no longer works.
            await _context.DisposeAsync();

            var asking = () => _service.GetCurrenciesAsync();

            await asking.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Warming_the_cache_up_is_the_read_the_first_caller_would_have_made()
        {
            await _service.InitializeCacheAsync();

            await _context.DisposeAsync();

            var asking = () => _service.GetCurrenciesAsync();

            await asking.Should().NotThrowAsync();
        }
    }
}
