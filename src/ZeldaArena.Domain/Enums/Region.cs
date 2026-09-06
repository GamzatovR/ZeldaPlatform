namespace ZeldaArena.Domain.Enums;

/// <summary>
/// Регион турнира или команды. Именно enum, а не строка: по нему фильтруют
/// (docs/SPEC.md §10.2) и его переводят на оба языка (§9.5).
/// </summary>
public enum Region
{
    Global = 0,
    Europe = 1,
    Cis = 2,
    NorthAmerica = 3,
    SouthAmerica = 4,
    Asia = 5,
    Oceania = 6,
}
