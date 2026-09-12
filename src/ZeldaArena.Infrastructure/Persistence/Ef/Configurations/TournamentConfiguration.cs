using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class TournamentConfiguration : EntityConfiguration<Tournament>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("Tournaments");

        builder.Property(tournament => tournament.Slug).IsRequired();
        builder.HasIndex(tournament => tournament.Slug).IsUnique();

        builder.Property(tournament => tournament.Name).IsRequired().HasMaxLength(200);
        builder.Property(tournament => tournament.Description).HasMaxLength(4000);
        builder.Property(tournament => tournament.LogoPath).HasMaxLength(400);
        builder.Property(tournament => tournament.BannerPath).HasMaxLength(400);

        // Призовой фонд — объект-значение: два столбца, отдельной таблицы нет.
        builder.ComplexProperty(tournament => tournament.PrizePool, money =>
        {
            money.Property(amount => amount.Amount).HasColumnName("PrizePool");
            money.Property(currency => currency.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsFixedLength();
        });

        // Индексы под фильтры списка турниров.
        builder.HasIndex(tournament => new { tournament.Status, tournament.StartsAt });
        builder.HasIndex(tournament => new { tournament.Region, tournament.Status });
        builder.HasIndex(tournament => tournament.IsFeatured);

        builder.HasMany(tournament => tournament.Participants)
            .WithOne(participant => participant.Tournament)
            .HasForeignKey(participant => participant.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: удаление турнира не должно каскадом стирать историю матчей.
        builder.HasMany(tournament => tournament.Matches)
            .WithOne(match => match.Tournament)
            .HasForeignKey(match => match.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Состав участников — часть агрегата.
        builder.Navigation(tournament => tournament.Participants)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.Navigation(tournament => tournament.Matches)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}