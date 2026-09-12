namespace ZeldaArena.Application.Features.Account.Queries.GetTwoFactorStatus;

public sealed record TwoFactorStatusDto(bool IsEnabled, int RemainingRecoveryCodes, bool IsRequired);