using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamLogo;

/// <summary>Новый логотип своей команды.</summary>
public sealed record ChangeMyTeamLogoCommand(Guid TeamId, FileUpload Logo) : ICommand, IAuditableRequest
{
    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}