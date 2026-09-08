using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.ChangePassword;

/// <summary>
/// Смена пароля из личного кабинета. Текущий пароль обязателен: без него доступ
/// к чужой незакрытой вкладке означал бы захват учётной записи (docs/SPEC.md §8.2).
///
/// Идентификатор пользователя в команде не передаётся — он берётся из текущего
/// запроса. Иначе кто угодно менял бы пароль кому угодно, подставив чужой Guid.
/// </summary>
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}