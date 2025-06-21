using EEaseWebAPI.Domain.Entities.Currency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface ICurrencyService
    {
        Task<List<AllWordCurrencies>> GetCurrenciesAsync();
        Task InitializeCacheAsync();
    }
}
