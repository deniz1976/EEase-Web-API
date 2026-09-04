using EEaseWebAPI.Domain.Entities.Route;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EEaseWebAPI.Persistence.Configurations
{
    public sealed class StandardRouteConfiguration : IEntityTypeConfiguration<StandardRoute>
    {
        public void Configure(EntityTypeBuilder<StandardRoute> builder)
        {
            builder.HasOne(route => route.User)
                .WithMany(user => user.MyRoutes)
                .HasForeignKey(route => route.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(route => route.LikedUsers)
                .WithMany(user => user.LikedRoutes)
                .UsingEntity(join => join.ToTable("UserLikedRoutes"));

            builder.HasMany(route => route.TravelDays)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(route => route.UserId);
            builder.HasIndex(route => route.Status);
        }
    }

    public sealed class TravelDayConfiguration : IEntityTypeConfiguration<TravelDay>
    {
        public void Configure(EntityTypeBuilder<TravelDay> builder)
        {
            builder.HasOne(day => day.User);

            builder.HasOne(day => day.Accomodation)
                .WithOne()
                .HasForeignKey<TravelAccomodation>("TravelDayId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(day => day.Breakfast)
                .WithOne()
                .HasForeignKey<Breakfast>("TravelDayId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(day => day.Lunch)
                .WithOne()
                .HasForeignKey<Lunch>("TravelDayId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(day => day.Dinner)
                .WithOne()
                .HasForeignKey<Dinner>("TravelDayId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(day => day.FirstPlace)
                .WithOne()
                .HasForeignKey<Place>("FirstPlaceTravelDayId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(day => day.SecondPlace)
                .WithOne()
                .HasForeignKey<Place>("SecondPlaceTravelDayId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(day => day.ThirdPlace)
                .WithOne()
                .HasForeignKey<Place>("ThirdPlaceTravelDayId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(day => day.PlaceAfterDinner)
                .WithOne()
                .HasForeignKey<PlaceAfterDinner>("TravelDayId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
