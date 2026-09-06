using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

/// <summary>Связь «турнир — команда» с составным первичным ключом (docs/SPEC.md §6).</summary>
public sealed class TournamentTeamConfiguration : IEntityTypeConfiguration<TournamentTeam>
{
    public void Configure(EntityTypeBuilder<TournamentTeam> builder)
    {
        builder.ToTable("TournamentTeams");

        builder.HasKey(participant => new { participant.TournamentId, participant.TeamId });

        builder.HasOne(participant => participant.Team)
            .WithMany()
            .HasForeignKey(participant => participant.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(participant => participant.TeamId);
    }
}