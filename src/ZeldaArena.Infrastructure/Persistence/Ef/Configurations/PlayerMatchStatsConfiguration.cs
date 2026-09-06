using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class PlayerMatchStatsConfiguration : EntityConfiguration<PlayerMatchStats>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PlayerMatchStats> builder)
    {
        builder.ToTable("PlayerMatchStats");

        builder.Property(stats => stats.Rating).HasPrecision(5, 2);

        builder.HasOne(stats => stats.Player)
            .WithMany()
            .HasForeignKey(stats => stats.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(stats => stats.Team)
            .WithMany()
            .HasForeignKey(stats => stats.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Статистика игрока в матче существует в одном экземпляре.
        builder.HasIndex(stats => new { stats.MatchId, stats.PlayerId }).IsUnique();
        builder.HasIndex(stats => stats.PlayerId);
    }
}