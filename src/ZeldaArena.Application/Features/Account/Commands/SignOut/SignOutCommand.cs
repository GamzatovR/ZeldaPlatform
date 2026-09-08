using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.SignOut;

/// <summary>
/// Выход из системы. Аудируется: выход входит в список обязательных к записи
/// событий docs/SPEC.md §8.2.
/// </summary>
public sealed record SignOutCommand : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    /// <summary>Кто именно вышел, аудит возьмёт из текущего пользователя запроса.</summary>
    public string? AuditEntityId => null;
}