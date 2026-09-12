using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class MatchConfiguration : EntityConfiguration<Match>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");

        builder.Property(match => match.StreamUrl).HasMaxLength(500);

        builder.Ignore(match => match.WinsRequired);
        builder.Ignore(match => match.IsFinished);

        // Два модератора могут править счёт одного матча одновременно —
        // токен конкурентности обязателен.
        builder.UseXminAsConcurrencyToken();

        // Restrict: история матчей переживает удаление команды.
        builder.HasOne(match => match.TeamA)
            .WithMany()
            .HasForeignKey(match => match.TeamAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(match => match.TeamB)
            .WithMany()
            .HasForeignKey(match => match.TeamBId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(match => match.PlayerStats)
            .WithOne(stats => stats.Match)
            .HasForeignKey(stats => stats.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(match => match.PlayerStats)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Индексы под расписание, вкладки матчей турнира и ленту live.
        builder.HasIndex(match => new { match.TournamentId, match.Status, match.ScheduledAt });
        builder.HasIndex(match => new { match.Status, match.ScheduledAt });
        builder.HasIndex(match => match.TeamAId);
        builder.HasIndex(match => match.TeamBId);
    }
}