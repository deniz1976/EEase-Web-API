using EEaseWebAPI.Domain.Entities.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface ICustomRouteService
    {
        Task<StandardRoute> CreateRandomRoute(string destination, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? _PRICE_LEVEL);

        Task<StandardRoute> CreatePrefRoute(string? destination,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? price_level,
            string? username,
            List<string>? friends);

    }
}
