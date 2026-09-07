namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Текущий пользователь и контекст его запроса. Нужен хендлерам для проверки владельца
/// (docs/SPEC.md §15, защита от IDOR) и behaviors для аудита: в записи обязаны быть
/// IP, User-Agent и correlation id (§12, §13).
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    string? CorrelationId { get; }

    /// <summary>
    /// Проверка роли — только для Admin, Moderator и User.
    /// Роль <c>Premium</c> здесь спрашивать запрещено: она существует ради бейджа
    /// в разметке, а доступ к платным функциям определяется исключительно фичами
    /// через <see cref="IEntitlementService"/> (docs/SPEC.md §7.4, §20 пункт 2).
    /// </summary>
    bool IsInRole(string role);
}