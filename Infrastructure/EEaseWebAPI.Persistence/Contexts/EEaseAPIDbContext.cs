using EEaseWebAPI.Domain.Entities.AllWorldCities;
using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Currency;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Contexts
{
    public class EEaseAPIDbContext : IdentityDbContext<AppUser,AppRole,string>
    {
        public EEaseAPIDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AllWorldCities>().HasNoKey();
            modelBuilder.Entity<AllWordCurrencies>().HasNoKey();

            modelBuilder.Entity<UserFriendship>(builder =>
            {
                builder.HasOne(f => f.Requester)
                    .WithMany(u => u.SentFriendRequests)
                    .HasForeignKey(f => f.RequesterId)
                    .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(f => f.Addressee)
                    .WithMany(u => u.ReceivedFriendRequests)
                    .HasForeignKey(f => f.AddresseeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StandardRoute>(builder =>
            {
                builder.HasOne(r => r.User)
                    .WithMany(u => u.MyRoutes)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(r => r.LikedUsers)
                    .WithMany(u => u.LikedRoutes)
                    .UsingEntity(j => j.ToTable("UserLikedRoutes"));

                builder.HasMany(r => r.TravelDays)
                    .WithOne()
                    .OnDelete(DeleteBehavior.Cascade);
            });




            modelBuilder.Entity<TravelDay>(builder =>
            {
                builder.HasOne(d => d.User);
                
                builder.HasOne(d => d.Accomodation)
                    .WithOne()
                    .HasForeignKey<TravelAccomodation>("TravelDayId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(d => d.Breakfast)
                    .WithOne()
                    .HasForeignKey<Breakfast>("TravelDayId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(d => d.Lunch)
                    .WithOne()
                    .HasForeignKey<Lunch>("TravelDayId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(d => d.Dinner)
                    .WithOne()
                    .HasForeignKey<Dinner>("TravelDayId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(d => d.FirstPlace)
                    .WithOne()
                    .HasForeignKey<Place>("FirstPlaceTravelDayId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(d => d.SecondPlace)
                    .WithOne()
                    .HasForeignKey<Place>("SecondPlaceTravelDayId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(d => d.ThirdPlace)
                    .WithOne()
                    .HasForeignKey<Place>("ThirdPlaceTravelDayId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(d => d.PlaceAfterDinner)
                    .WithOne()
                    .HasForeignKey<PlaceAfterDinner>("TravelDayId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BaseRestaurantPlaceEntity>().UseTptMappingStrategy();
            modelBuilder.Entity<BaseTravelPlaceEntity>().UseTptMappingStrategy();

            modelBuilder.Entity<BaseRestaurantPlaceEntity>(builder =>
            {
                builder.HasOne(e => e.Location)
                    .WithOne()
                    .HasForeignKey<Location>("BaseRestaurantPlaceEntityId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(e => e.RegularOpeningHours)
                    .WithOne()
                    .HasForeignKey<RegularOpeningHours>("BaseRestaurantPlaceEntityId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(e => e.DisplayName)
                    .WithOne()
                    .HasForeignKey<DisplayName>("BaseRestaurantPlaceEntityId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(e => e.PaymentOptions)
                    .WithOne()
                    .HasForeignKey<PaymentOptions>("BaseRestaurantPlaceEntityId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(e => e.Photos)
                    .WithOne()
                    .HasForeignKey("BaseRestaurantPlaceEntityId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BaseTravelPlaceEntity>(builder =>
            {
                builder.HasOne(e => e.Location)
                    .WithOne()
                    .HasForeignKey<Location>("BaseTravelPlaceEntityId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(e => e.RegularOpeningHours)
                    .WithOne()
                    .HasForeignKey<RegularOpeningHours>("BaseTravelPlaceEntityId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(e => e.DisplayName)
                    .WithOne()
                    .HasForeignKey<DisplayName>("BaseTravelPlaceEntityId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(e => e.PaymentOptions)
                    .WithOne()
                    .HasForeignKey<PaymentOptions>("BaseTravelPlaceEntityId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(e => e.Photos)
                    .WithOne()
                    .HasForeignKey("BaseTravelPlaceEntityId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RegularOpeningHours>(builder =>
            {
                builder.HasMany(e => e.Periods)
                    .WithOne()
                    .HasForeignKey("RegularOpeningHoursId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Period>(builder =>
            {
                builder.HasOne(e => e.Open)
                    .WithOne()
                    .HasForeignKey<Open>("PeriodId")
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(e => e.Close)
                    .WithOne()
                    .HasForeignKey<Close>("PeriodId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<StandardRoute> StandardRoutes { get; set; }
        public DbSet<TravelDay> TravelDays { get; set; }
        public DbSet<Breakfast> Breakfasts { get; set; }
        public DbSet<Lunch> Lunches { get; set; }
        public DbSet<Dinner> Dinners { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<PlaceAfterDinner> PlacesAfterDinner { get; set; }
        public DbSet<TravelAccomodation> TravelAccomodations { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Weather> Weathers { get; set; }
        public DbSet<Photos> Photos { get; set; }
        public DbSet<DisplayName> DisplayNames { get; set; }
        public DbSet<PaymentOptions> PaymentOptions { get; set; }
        public DbSet<RegularOpeningHours> RegularOpeningHours { get; set; }
        public DbSet<Period> Periods { get; set; }
        public DbSet<Open> Opens { get; set; }
        public DbSet<Close> Closes { get; set; }
        public DbSet<AllWorldCities> AllWorldCities { get; set; }
        public DbSet<AllWordCurrencies> Currencies { get; set; }
        public DbSet<UserPersonalization> UserPersonalizations { get; set; }
        public DbSet<UserFoodPreferences> UserFoodPreferences { get; set; }
        public DbSet<UserAccommodationPreferences> UserAccommodationPreferences { get; set; }   
        public DbSet<UserFriendship> UserFriendships { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var datas = ChangeTracker.Entries<BaseEntity>();
            foreach (var data in datas)
            {
                _ = data.State switch
                {
                    EntityState.Added => data.Entity.CreatedDate = DateTime.UtcNow,
                    EntityState.Modified => data.Entity.UpdatedDate = DateTime.UtcNow,
                    _ => DateTime.UtcNow
                };
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
