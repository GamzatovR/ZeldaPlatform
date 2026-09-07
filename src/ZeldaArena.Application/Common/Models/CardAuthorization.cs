namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Всё, что остаётся от карты после авторизации и что вообще разрешено хранить
/// (docs/SPEC.md §7.6): бренд, определённый по BIN, и последние четыре цифры.
/// Именно эти два значения попадают в <c>Payments</c>.
/// </summary>
public sealed record CardAuthorization(string CardBrand, string CardLast4);