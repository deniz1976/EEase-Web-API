using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EEaseWebAPI.Persistence.Configurations
{
    public sealed class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
    {
        public void Configure(EntityTypeBuilder<UserBlock> builder)
        {
            builder.HasOne(block => block.Blocker)
                .WithMany(user => user.BlocksIssued)
                .HasForeignKey(block => block.BlockerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(block => block.Blocked)
                .WithMany(user => user.BlocksReceived)
                .HasForeignKey(block => block.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(block => new { block.BlockerId, block.BlockedId })
                .IsUnique();

            builder.HasIndex(block => block.BlockedId);
        }
    }
}
