using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.CreateTournament;

/// <summary>Анонс нового турнира из админки (docs/SPEC.md §9.4, п. 2). Возвращает его идентификатор.</summary>
public sealed record CreateTournamentCommand : ICommand<Guid>, IAuditableRequest, ITournamentFields
{
    public required string Name { get; init; }

    public TournamentTier Tier { get; init; }

    public Region Region { get; init; }

    public decimal PrizePool { get; init; }

    public DateTimeOffset StartsAt { get; init; }

    public DateTimeOffset EndsAt { get; init; }

    public string? Description { get; init; }

    public string? RulesHtml { get; init; }

    public bool IsFeatured { get; init; }

    public FileUpload? Logo { get; init; }

    public string AuditEntityType => nameof(Tournament);

    public string? AuditEntityId => null;
}