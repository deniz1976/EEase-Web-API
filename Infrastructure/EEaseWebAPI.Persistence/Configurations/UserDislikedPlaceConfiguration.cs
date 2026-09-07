using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EEaseWebAPI.Persistence.Configurations
{
    public sealed class UserDislikedPlaceConfiguration : IEntityTypeConfiguration<UserDislikedPlace>
    {
        public void Configure(EntityTypeBuilder<UserDislikedPlace> builder)
        {
            builder.HasOne(disliked => disliked.User)
                .WithMany()
                .HasForeignKey(disliked => disliked.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(disliked => new { disliked.UserId, disliked.GoogleId })
                .IsUnique();
        }
    }
}
