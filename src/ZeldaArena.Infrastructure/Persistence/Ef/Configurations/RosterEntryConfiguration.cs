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
    }
}