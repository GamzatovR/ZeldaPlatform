using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.ConfirmEmailChange;

/// <summary>
/// Подтверждение нового адреса по ссылке из письма. До этого момента адрес
/// учётной записи не меняется (docs/SPEC.md §8.2).
/// </summary>
public sealed record ConfirmEmailChangeCommand(Guid UserId, string NewEmail, string Token)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => UserId.ToString();
}