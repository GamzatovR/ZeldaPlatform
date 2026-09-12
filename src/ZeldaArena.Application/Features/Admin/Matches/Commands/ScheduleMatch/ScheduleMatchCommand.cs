using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.ScheduleMatch;

/// <summary>Новый матч в сетке турнира (docs/SPEC.md §9.4, п. 3). Возвращает его идентификатор.</summary>
public sealed record ScheduleMatchCommand : ICommand<Guid>, IAuditableRequest
{
    public Guid TournamentId { get; init; }

    public Guid TeamAId { get; init; }

    public Guid TeamBId { get; init; }

    public DateTimeOffset ScheduledAt { get; init; }

    public int BestOf { get; init; } = 3;

    public string? StreamUrl { get; init; }

    public string AuditEntityType => nameof(Match);

    public string? AuditEntityId => null;
}