using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(Guid UserId, string Token)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => UserId.ToString();
}