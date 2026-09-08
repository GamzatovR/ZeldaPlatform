using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.RegisterUser;

/// <summary>
/// Регистрация пользователя (docs/SPEC.md §8.2). Учётная запись заводится сразу,
/// но войти по ней нельзя до подтверждения адреса: ссылка уходит письмом.
///
/// Команда аудируется — регистрация в списке обязательных к записи событий §8.2.
/// Пароль в запись не попадёт: имя поля закрыто SensitiveProperties.
/// </summary>
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