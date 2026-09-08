using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.SignInWithRecoveryCode;

/// <summary>
/// Вход по коду восстановления, когда аутентификатор недоступен. Код одноразовый:
/// Identity гасит его при использовании (docs/SPEC.md §8.2).
/// </summary>
public sealed record SignInWithRecoveryCodeCommand(string RecoveryCode)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}