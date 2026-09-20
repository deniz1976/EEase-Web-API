using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.AllWorldCities;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EEaseWebAPI.Persistence.Repositories
{
    public sealed class AllWorldCitiesRepository : IAllWorldCitiesRepository
    {
        private readonly EEaseAPIDbContext _context;
        private readonly ILogger<AllWorldCitiesRepository> _logger;

        public AllWorldCitiesRepository(EEaseAPIDbContext context, ILogger<AllWorldCitiesRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AllWorldCities>> GetAllCitiesAsync()
        {
            var cities = await _context.AllWorldCities
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("Read {Count} cities from the database.", cities.Count);

            return cities;
        }
    }
}
