namespace ZeldaArena.Application.Common.Models.Identity;

public sealed record TwoFactorSetup(string SharedKey, string AuthenticatorUri);