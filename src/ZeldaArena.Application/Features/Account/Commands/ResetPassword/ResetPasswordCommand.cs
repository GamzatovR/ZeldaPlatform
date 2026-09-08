using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.ResetPassword;

/// <summary>
/// Установка нового пароля по ссылке из письма. Аудируется: смена пароля —
/// событие из обязательного списка docs/SPEC.md §8.2.
/// </summary>
public sealed record ResetPasswordCommand(Guid UserId, string Token, string NewPassword)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => UserId.ToString();
}