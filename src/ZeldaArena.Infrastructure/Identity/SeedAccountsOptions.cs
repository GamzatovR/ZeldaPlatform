namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Учётные записи для сида (docs/SPEC.md §6): администратор и демонстрационные
/// пользователи, на которых видно разграничение прав.
///
/// Подписки демо-пользователям раздаёт Фаза 4 — здесь только сами учётные записи.
/// </summary>
public sealed class SeedAccountsOptions
{
    public const string SectionName = "SeedAccounts";

    public SeedAccountOptions Admin { get; set; } = new();

    public SeedAccountOptions Moderator { get; set; } = new();

    public IList<SeedAccountOptions> Demo { get; set; } = [];
}