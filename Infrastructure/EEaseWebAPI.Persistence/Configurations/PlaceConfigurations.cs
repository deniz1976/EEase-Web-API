using EEaseWebAPI.Domain.Entities.Route;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EEaseWebAPI.Persistence.Configurations
{
    public sealed class BaseRestaurantPlaceEntityConfiguration
        : IEntityTypeConfiguration<BaseRestaurantPlaceEntity>
    {
        public void Configure(EntityTypeBuilder<BaseRestaurantPlaceEntity> builder)
        {
            builder.UseTptMappingStrategy();

            builder.HasOne(place => place.Location)
                .WithOne()
                .HasForeignKey<Location>("BaseRestaurantPlaceEntityId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(place => place.RegularOpeningHours)
                .WithOne()
                .HasForeignKey<RegularOpeningHours>("BaseRestaurantPlaceEntityId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(place => place.DisplayName)
                .WithOne()
                .HasForeignKey<DisplayName>("BaseRestaurantPlaceEntityId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(place => place.PaymentOptions)
                .WithOne()
                .HasForeignKey<PaymentOptions>("BaseRestaurantPlaceEntityId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(place => place.Photos)
                .WithOne()
                .HasForeignKey("BaseRestaurantPlaceEntityId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public sealed class BaseTravelPlaceEntityConfiguration
        : IEntityTypeConfiguration<BaseTravelPlaceEntity>
    {
        public void Configure(EntityTypeBuilder<BaseTravelPlaceEntity> builder)
        {
            builder.UseTptMappingStrategy();

            builder.HasOne(place => place.Location)
                .WithOne()
                .HasForeignKey<Location>("BaseTravelPlaceEntityId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(place => place.RegularOpeningHours)
                .WithOne()
                .HasForeignKey<RegularOpeningHours>("BaseTravelPlaceEntityId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(place => place.DisplayName)
                .WithOne()
                .HasForeignKey<DisplayName>("BaseTravelPlaceEntityId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(place => place.PaymentOptions)
                .WithOne()
                .HasForeignKey<PaymentOptions>("BaseTravelPlaceEntityId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(place => place.Photos)
                .WithOne()
                .HasForeignKey("BaseTravelPlaceEntityId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public sealed class RegularOpeningHoursConfiguration : IEntityTypeConfiguration<RegularOpeningHours>
    {
        public void Configure(EntityTypeBuilder<RegularOpeningHours> builder)
        {
            builder.HasMany(hours => hours.Periods)
                .WithOne()
                .HasForeignKey("RegularOpeningHoursId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public sealed class PeriodConfiguration : IEntityTypeConfiguration<Period>
    {
        public void Configure(EntityTypeBuilder<Period> builder)
        {
            builder.HasOne(period => period.Open)
                .WithOne()
                .HasForeignKey<Open>("PeriodId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(period => period.Close)
                .WithOne()
                .HasForeignKey<Close>("PeriodId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
