using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.ConfirmEmail;

/// <summary>
/// Подтверждение адреса по ссылке из письма (docs/SPEC.md §8.2). До этого момента
/// вход запрещён: SignIn.RequireConfirmedEmail включён.
/// </summary>
public sealed record ConfirmEmailCommand(Guid UserId, string Token)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => UserId.ToString();
}