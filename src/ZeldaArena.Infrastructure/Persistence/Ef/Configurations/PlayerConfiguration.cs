using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class PlayerConfiguration : EntityConfiguration<Player>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");

        builder.Property(player => player.Slug).IsRequired();
        builder.HasIndex(player => player.Slug).IsUnique();

        builder.Property(player => player.Nickname).IsRequired().HasMaxLength(60);
        builder.Property(player => player.FirstName).HasMaxLength(80);
        builder.Property(player => player.LastName).HasMaxLength(80);
        builder.Property(player => player.Country).IsRequired();
        builder.Property(player => player.AvatarPath).HasMaxLength(400);
        builder.Property(player => player.Bio).HasMaxLength(4000);

        // Индексы под фильтры списка игроков: роль, страна, поиск по нику.
        builder.HasIndex(player => player.Role);
        builder.HasIndex(player => player.Country);
        builder.HasIndex(player => player.Nickname);

        builder.HasMany(player => player.RosterEntries)
            .WithOne(entry => entry.Player)
            .HasForeignKey(entry => entry.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(player => player.RosterEntries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}