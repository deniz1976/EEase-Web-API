using EEaseWebAPI.Domain.Entities.AllWorldCities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Repositories
{
    public interface IAllWorldCitiesRepository
    {
        Task<List<AllWorldCities>> GetAllCitiesAsync();
    }
} 