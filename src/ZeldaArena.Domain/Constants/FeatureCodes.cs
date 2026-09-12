namespace ZeldaArena.Domain.Constants;

/// <summary>Коды платных функций.</summary>
public static class FeatureCodes
{
    /// <summary>Создание своей команды и управление её составом.</summary>
    public const string TeamCreate = "team.create";

    /// <summary>Расширенная статистика команд и игроков.</summary>
    public const string StatsAdvanced = "stats.advanced";

    public static IReadOnlyList<string> All { get; } = [TeamCreate, StatsAdvanced];
}