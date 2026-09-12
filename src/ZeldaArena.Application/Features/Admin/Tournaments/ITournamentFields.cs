using ZeldaArena.Application.Common.Files;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Tournaments;

/// <summary>
/// Поля карточки турнира — общие для создания и правки. Общий интерфейс нужен ради
/// одного набора правил (<see cref="TournamentFieldsValidator"/>): две копии разошлись
/// бы, и правка принимала бы то, что отвергает создание.
/// </summary>
public interface ITournamentFields
{
    string Name { get; }

    TournamentTier Tier { get; }

    Region Region { get; }

    decimal PrizePool { get; }

    DateTimeOffset StartsAt { get; }

    DateTimeOffset EndsAt { get; }

    string? Description { get; }

    /// <summary>Регламент в HTML. Очищается санитайзером в хендлере, до сохранения (§15).</summary>
    string? RulesHtml { get; }

    bool IsFeatured { get; }

    FileUpload? Logo { get; }
}