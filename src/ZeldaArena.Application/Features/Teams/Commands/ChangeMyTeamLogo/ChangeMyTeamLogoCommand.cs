using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamLogo;

/// <summary>Новый логотип своей команды (docs/SPEC.md §9.3, п. 8; §15 — правила загрузки).</summary>
public sealed record ChangeMyTeamLogoCommand(Guid TeamId, FileUpload Logo) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}