namespace ZeldaArena.Application.Common.Models.Identity;

/// <summary>
/// Фильтр таблицы пользователей. Порядок задаётся флагом, а не строкой-ключом: выборку
/// собирает Infrastructure, и имя поля из адреса до неё не доходит (§10.2, whitelist).
/// </summary>
/// <param name="Search">Часть адреса или отображаемого имени.</param>
/// <param name="Role">Роль; пусто — любая.</param>
/// <param name="IsBlocked">Только заблокированные или только активные.</param>
/// <param name="NewestFirst">Сначала недавно зарегистрированные.</param>
/// <param name="Page">Страница, начиная с единицы.</param>
/// <param name="PageSize">Размер страницы.</param>
public sealed record AdminUserFilter(
    string? Search,
    string? Role,
    bool? IsBlocked,
    bool NewestFirst,
    int Page,
    int PageSize);