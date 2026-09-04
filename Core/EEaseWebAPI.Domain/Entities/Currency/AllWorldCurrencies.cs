using Microsoft.EntityFrameworkCore;

namespace EEaseWebAPI.Domain.Entities.Currency
{
    [Keyless]
    public class AllWorldCurrencies
    {
        public string? Entity { get; set; }

        public string? Currency { get; set; }

        public string? AlphabeticCode { get; set; }
    }
}
