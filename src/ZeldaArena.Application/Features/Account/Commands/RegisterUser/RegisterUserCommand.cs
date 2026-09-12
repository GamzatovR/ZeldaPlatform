using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string? DisplayName,
    string? PreferredCulture)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    /// <summary>Идентификатора на момент вызова ещё нет — пользователь только создаётся.</summary>
    public string? AuditEntityId => null;
}