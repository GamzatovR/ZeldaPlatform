using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.ChangeMatchStatus;

/// <summary>
/// Старт, завершение и отмена матча из пульта (docs/SPEC.md §9.4, п. 3). Аудируется
/// наравне с правкой счёта: это то же вмешательство в ход матча (§13).
/// </summary>
public sealed record ChangeMatchStatusCommand(Guid Id, MatchTransition Transition)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Match);

    public string? AuditEntityId => Id.ToString();
}