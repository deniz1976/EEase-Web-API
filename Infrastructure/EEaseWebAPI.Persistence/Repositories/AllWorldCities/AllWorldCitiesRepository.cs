using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.AllWorldCities;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Repositories
{
    public class AllWorldCitiesRepository : IAllWorldCitiesRepository
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
            try
            {
                _logger.LogInformation("Fetching all cities from database...");
                var cities = await _context.AllWorldCities
                    .AsNoTracking()
                    .ToListAsync();
                
                _logger.LogInformation($"Successfully fetched {cities.Count} cities from database.");
                return cities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cities from database");
                throw;
            }
        }
    }
} 