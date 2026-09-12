namespace ZeldaArena.Application.Common.Models;

public static class PageSizes
{
    public const int Default = 12;

    public static readonly IReadOnlyList<int> Allowed = [12, 24, 48];

    public static int Normalize(int requested) =>
        Allowed.Contains(requested) ? requested : Default;
}