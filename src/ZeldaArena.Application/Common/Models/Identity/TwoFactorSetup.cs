namespace ZeldaArena.Application.Common.Models.Identity;

/// <summary>
/// Данные для подключения аутентификатора (docs/SPEC.md §8.2): ключ для ручного ввода
/// и ссылка otpauth://, из которой страница рисует QR-код.
///
/// Оба поля — секрет. Имена подобраны так, чтобы их закрывал SensitiveProperties:
/// попадание ключа в лог или в аудит равносильно выдаче второго фактора (§13).
/// </summary>
public sealed record TwoFactorSetup(string SharedKey, string AuthenticatorUri);