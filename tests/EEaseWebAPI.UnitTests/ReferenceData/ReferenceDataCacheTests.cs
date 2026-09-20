using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Persistence.Services.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace EEaseWebAPI.UnitTests.ReferenceData
{
    public class ReferenceDataCacheTests
    {
        private readonly ReferenceDataCache _cache = new(
            new MemoryCache(new MemoryCacheOptions()),
            Options.Create(new CacheOptions()));

        [Fact]
        public async Task The_data_is_read_once_and_kept()
        {
            var reads = 0;

            Task<List<string>> Load() => Task.FromResult(new List<string> { $"read {++reads}" });

            (await _cache.GetOrLoadAsync("cities", Load)).Should().Equal("read 1");
            (await _cache.GetOrLoadAsync("cities", Load)).Should().Equal("read 1");

            reads.Should().Be(1);
        }

        [Fact]
        public async Task Two_kinds_of_data_do_not_evict_each_other()
        {
            var cityReads = 0;
            var countryReads = 0;

            await _cache.GetOrLoadAsync("cities", () =>
            {
                cityReads++;
                return Task.FromResult(new List<City> { new("Rome") });
            });

            await _cache.GetOrLoadAsync("countries", () =>
            {
                countryReads++;
                return Task.FromResult(new List<string> { "Italy" });
            });

            // Reading them again must not go back to the database: the two used to share a
            // key, and each read replaced the other because the shapes did not match.
            await _cache.GetOrLoadAsync("cities", () =>
            {
                cityReads++;
                return Task.FromResult(new List<City> { new("Rome") });
            });

            await _cache.GetOrLoadAsync("countries", () =>
            {
                countryReads++;
                return Task.FromResult(new List<string> { "Italy" });
            });

            cityReads.Should().Be(1);
            countryReads.Should().Be(1);
        }

        [Fact]
        public async Task Nothing_is_loaded_until_it_is_asked_for()
        {
            _cache.IsLoaded("cities").Should().BeFalse();

            await _cache.GetOrLoadAsync("cities", () => Task.FromResult(new List<string> { "Rome" }));

            _cache.IsLoaded("cities").Should().BeTrue();
        }

        [Fact]
        public void Every_kind_of_reference_data_has_a_key_of_its_own()
        {
            // The city list, the city names and the countries shared one key once, so
            // warming them up left only the last one cached and the other two read from the
            // database on every request.
            var keys = new CacheOptions();

            new[]
            {
                keys.AllCitiesCacheKey,
                keys.CityNamesCacheKey,
                keys.AllCountriesCacheKey,
                keys.AllCurrenciesCacheKey,
                keys.UsersCacheKey
            }.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void The_keys_come_from_configuration()
        {
            var cache = new ReferenceDataCache(
                new MemoryCache(new MemoryCacheOptions()),
                Options.Create(new CacheOptions { AllCitiesCacheKey = "custom" }));

            cache.Keys.AllCitiesCacheKey.Should().Be("custom");
        }

        private sealed record City(string Name);
    }
}
