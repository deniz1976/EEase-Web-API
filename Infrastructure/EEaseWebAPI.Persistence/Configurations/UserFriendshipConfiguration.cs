using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EEaseWebAPI.Persistence.Configurations
{
    public sealed class UserFriendshipConfiguration : IEntityTypeConfiguration<UserFriendship>
    {
        public void Configure(EntityTypeBuilder<UserFriendship> builder)
        {
            builder.HasOne(friendship => friendship.Requester)
                .WithMany(user => user.SentFriendRequests)
                .HasForeignKey(friendship => friendship.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(friendship => friendship.Addressee)
                .WithMany(user => user.ReceivedFriendRequests)
                .HasForeignKey(friendship => friendship.AddresseeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(friendship => new { friendship.RequesterId, friendship.AddresseeId })
                .IsUnique();

            builder.HasIndex(friendship => new { friendship.AddresseeId, friendship.Status });
            builder.HasIndex(friendship => new { friendship.RequesterId, friendship.Status });
        }
    }
}
