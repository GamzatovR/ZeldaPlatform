using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Esports;
using ZeldaArena.Infrastructure.Identity;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class FollowConfiguration : EntityConfiguration<Follow>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("Follows");

        // Повторный Follow не должен создавать дубль.
        builder.HasIndex(follow => new { follow.UserId, follow.TargetType, follow.TargetId })
            .IsUnique();

        builder.HasIndex(follow => new { follow.TargetType, follow.TargetId });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(follow => follow.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}