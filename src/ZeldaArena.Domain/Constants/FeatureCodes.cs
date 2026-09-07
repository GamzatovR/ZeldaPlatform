namespace ZeldaArena.Domain.Constants;

/// <summary>
/// Коды платных функций. Строковых литералов кода фичи в проекте быть не должно:
/// доступ проверяется только через IEntitlementService по этим константам (CLAUDE.md).
/// Новая фича добавляется строкой в таблицу Features через админку — константа нужна
/// лишь тем фичам, на которые ссылается сам код (docs/SPEC.md §5.4, EP-3).
/// </summary>
public static class FeatureCodes
{
    /// <summary>Создание своей команды и управление её составом.</summary>
    public const string TeamCreate = "team.create";

    /// <summary>Расширенная статистика команд и игроков.</summary>
    public const string StatsAdvanced = "stats.advanced";

    public static IReadOnlyList<string> All { get; } = [TeamCreate, StatsAdvanced];
}