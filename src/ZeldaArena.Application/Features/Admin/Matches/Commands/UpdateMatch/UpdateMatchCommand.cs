using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.UpdateMatch;

/// <summary>
/// Время и трансляция матча. Команды и формат серии после назначения не меняются:
/// это уже сыгранная или назначенная пара, а не черновик — неверную пару проще
/// отменить и поставить заново.
/// </summary>
public sealed record UpdateMatchCommand(Guid Id, DateTimeOffset ScheduledAt, string? StreamUrl)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Match);

    public string? AuditEntityId => Id.ToString();
}