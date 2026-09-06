using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Esports;
using ZeldaArena.Infrastructure.Identity;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class TeamConfiguration : EntityConfiguration<Team>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");

        builder.Property(team => team.Slug).IsRequired();
        builder.HasIndex(team => team.Slug).IsUnique();

        builder.Property(team => team.Name).IsRequired().HasMaxLength(120);
        builder.Property(team => team.Tag).IsRequired().HasMaxLength(8);
        builder.Property(team => team.Country).IsRequired();
        builder.Property(team => team.LogoPath).HasMaxLength(400);
        builder.Property(team => team.Description).HasMaxLength(4000);

        builder.Ignore(team => team.ActiveRoster);

        builder.HasIndex(team => new { team.Region, team.Rating });
        builder.HasIndex(team => team.OwnerUserId);
        builder.HasIndex(team => team.IsApproved);

        // Владелец-подписчик. При удалении пользователя команда остаётся, но становится ничьей.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(team => team.OwnerUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(team => team.RosterEntries)
            .WithOne(entry => entry.Team)
            .HasForeignKey(entry => entry.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(team => team.RosterEntries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}