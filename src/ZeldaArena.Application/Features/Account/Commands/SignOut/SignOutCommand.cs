using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.SignOut;

public sealed record SignOutCommand : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    /// <summary>Кто именно вышел, аудит возьмёт из текущего пользователя запроса.</summary>
    public string? AuditEntityId => null;
}