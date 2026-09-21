using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EEaseWebAPI.Persistence.Configurations
{
    public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            // Every refresh reads one row by this and nothing else, and without an index
            // that is a scan of the users table on every token renewal.
            builder.HasIndex(user => user.RefreshTokenHash);
        }
    }
}
