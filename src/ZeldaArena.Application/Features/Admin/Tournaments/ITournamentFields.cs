using ZeldaArena.Application.Common.Files;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Tournaments;

public interface ITournamentFields
{
    string Name { get; }

    TournamentTier Tier { get; }

    Region Region { get; }

    decimal PrizePool { get; }

    DateTimeOffset StartsAt { get; }

    DateTimeOffset EndsAt { get; }

    string? Description { get; }

    /// <summary>Регламент в HTML. Очищается санитайзером в хендлере, до сохранения.</summary>
    string? RulesHtml { get; }

    bool IsFeatured { get; }

    FileUpload? Logo { get; }
}