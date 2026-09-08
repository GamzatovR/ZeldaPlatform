using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.RequestEmailChange;

/// <summary>
/// Запрос смены адреса. Идентификатор берётся из текущего запроса, а не из формы:
/// иначе можно было бы затеять смену адреса чужой учётной записи (docs/SPEC.md §15, IDOR).
/// </summary>
public sealed record RequestEmailChangeCommand(string NewEmail) : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}