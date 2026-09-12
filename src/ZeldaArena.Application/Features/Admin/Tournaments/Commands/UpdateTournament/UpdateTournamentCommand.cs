using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.UpdateTournament;

/// <summary>Правка карточки турнира. Слаг не меняется: это адрес страницы (ADR-0008).</summary>
public sealed record UpdateTournamentCommand : ICommand, IAuditableRequest, ITournamentFields
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public TournamentTier Tier { get; init; }

    public Region Region { get; init; }

    public decimal PrizePool { get; init; }

    public DateTimeOffset StartsAt { get; init; }

    public DateTimeOffset EndsAt { get; init; }

    public string? Description { get; init; }

    public string? RulesHtml { get; init; }

    public bool IsFeatured { get; init; }

    /// <summary>Новый логотип; <see langword="null"/> — оставить прежний.</summary>
    public FileUpload? Logo { get; init; }

    /// <summary>Убрать логотип без замены.</summary>
    public bool RemoveLogo { get; init; }

    public string AuditEntityType => nameof(Tournament);

    public string? AuditEntityId => Id.ToString();
}