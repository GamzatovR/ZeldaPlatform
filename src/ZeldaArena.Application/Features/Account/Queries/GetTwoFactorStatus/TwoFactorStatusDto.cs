namespace ZeldaArena.Application.Features.Account.Queries.GetTwoFactorStatus;

/// <summary>
/// Состояние второго фактора для страницы кабинета: включён ли, сколько кодов
/// восстановления осталось и обязателен ли он для этой учётной записи (docs/SPEC.md §8.2).
/// </summary>
public sealed record TwoFactorStatusDto(bool IsEnabled, int RemainingRecoveryCodes, bool IsRequired);