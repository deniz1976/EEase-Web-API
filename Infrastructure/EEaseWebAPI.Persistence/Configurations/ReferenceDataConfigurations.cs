using EEaseWebAPI.Domain.Entities.AllWorldCities;
using EEaseWebAPI.Domain.Entities.Currency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EEaseWebAPI.Persistence.Configurations
{
    public sealed class AllWorldCitiesConfiguration : IEntityTypeConfiguration<AllWorldCities>
    {
        public void Configure(EntityTypeBuilder<AllWorldCities> builder)
        {
            builder.HasNoKey();

            builder.HasIndex(city => city.city);
            builder.HasIndex(city => city.country);
        }
    }

    public sealed class AllWorldCurrenciesConfiguration : IEntityTypeConfiguration<AllWorldCurrencies>
    {
        public void Configure(EntityTypeBuilder<AllWorldCurrencies> builder)
        {
            builder.HasNoKey();
            builder.HasIndex(currency => currency.AlphabeticCode);
        }
    }
}
