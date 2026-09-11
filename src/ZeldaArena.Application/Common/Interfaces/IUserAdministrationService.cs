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
}