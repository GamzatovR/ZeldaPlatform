using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.DisableTwoFactor;

/// <summary>Отключение второго фактора. Ключ аутентификатора при этом сбрасывается.</summary>
public sealed record DisableTwoFactorCommand : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}