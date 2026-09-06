namespace ZeldaArena.Domain.Enums;

/// <summary>Состояние матча. Названия локализуются через SharedResource (docs/SPEC.md §9.5).</summary>
public enum MatchStatus
{
    Scheduled = 0,
    Live = 1,
    Finished = 2,
    Postponed = 3,
    Canceled = 4,
}
