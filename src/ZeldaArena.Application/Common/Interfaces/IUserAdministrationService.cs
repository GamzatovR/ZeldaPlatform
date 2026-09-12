using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Пользователи глазами администратора (docs/SPEC.md §9.4, страницы 1 и 10): сколько
/// их, список с фильтрами, роли, блокировка.
///
/// Отдельный порт, а не расширение <see cref="IUserAccountService"/>, — это ISP из §5.5:
/// сценариям самообслуживания (вход, смена пароля) незачем видеть методы, которыми один
/// пользователь управляет другими, а их тестам — подставлять под них заглушки.
///
/// Учётные записи живут в таблицах Identity, а не в доменных сущностях, поэтому
/// выборка собирается в Infrastructure: <c>ApplicationUser</c> в Application недоступен
/// (правило 2 §5.2).
/// </summary>
public interface IUserAdministrationService
{
    Task<int> CountUsersAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<AdminUserRowDto>> SearchAsync(
        AdminUserFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Блокирует или разблокирует учётную запись. Блокировка обязана обесценить
    /// действующие cookie: у сессии есть срок жизни, и без смены стампа безопасности
    /// заблокированный доработал бы до его истечения (docs/PROGRESS.md, Фаза 3).
    /// </summary>
    Task<Result> SetBlockedAsync(Guid userId, bool isBlocked, CancellationToken cancellationToken = default);

    /// <summary>
    /// Назначает пользователю ровно этот набор ролей. Стамп безопасности меняется
    /// по той же причине: cookie хранит роли на момент входа.
    /// </summary>
    Task<Result> SetRolesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default);

    /// <summary>Сколько всего пользователей с этой ролью — чтобы не разжаловать последнего администратора.</summary>
    Task<int> CountInRoleAsync(string role, CancellationToken cancellationToken = default);
}