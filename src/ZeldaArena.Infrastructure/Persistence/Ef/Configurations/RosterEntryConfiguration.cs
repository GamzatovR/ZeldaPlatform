using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class RosterEntryConfiguration : EntityConfiguration<RosterEntry>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RosterEntry> builder)
    {
        builder.ToTable("RosterEntries");

        // IsActive выводится из LeftAt и в базе не хранится: отдельный столбец
        // дублировал бы данные и мог бы с ними разойтись (docs/erd.md).
        builder.Ignore(entry => entry.IsActive);

        builder.HasIndex(entry => new { entry.TeamId, entry.LeftAt });
        builder.HasIndex(entry => new { entry.PlayerId, entry.LeftAt });

        // Игрок не может состоять в двух командах одновременно (docs/SPEC.md §15).
        // Сущность Team чужих составов не видит, поэтому правило между агрегатами
        // проверяет валидатор сценария — он даёт понятное сообщение. Индекс закрывает
        // то, чего валидатор не может: два одновременных запроса, каждый из которых
        // успел убедиться, что игрок свободен.
        builder.HasIndex(entry => entry.PlayerId)
            .IsUnique()
            .HasFilter("\"LeftAt\" IS NULL")
            .HasDatabaseName("IX_RosterEntries_PlayerId_Active");
    }
}