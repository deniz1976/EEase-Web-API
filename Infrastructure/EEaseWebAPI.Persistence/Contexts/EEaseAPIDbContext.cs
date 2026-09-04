using System.Reflection;
using EEaseWebAPI.Domain.Entities.AllWorldCities;
using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Currency;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EEaseWebAPI.Persistence.Contexts
{
    public class EEaseAPIDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public EEaseAPIDbContext(DbContextOptions options) : base(options)
        {
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
        public DbSet<AllWorldCurrencies> Currencies { get; set; }
        public DbSet<UserPersonalization> UserPersonalizations { get; set; }
        public DbSet<UserFoodPreferences> UserFoodPreferences { get; set; }
        public DbSet<UserAccommodationPreferences> UserAccommodationPreferences { get; set; }
        public DbSet<UserFriendship> UserFriendships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override int SaveChanges()
        {
            ApplyAuditTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditTimestamps()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = now;
                        entry.Entity.UpdatedDate = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedDate = now;
                        break;
                }
            }
        }
    }
}
