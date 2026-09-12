using FluentValidation;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Common.Validation;

public static class AccountValidationRules
{
    public const int MaxEmailLength = 256;

    /// <summary>Длина кода TOTP: шесть цифр.</summary>
    public const int TwoFactorCodeLength = 6;

    /// <summary>Коды восстановления Identity выдаёт в виде xxxxx-xxxxx.</summary>
    public const int MinRecoveryCodeLength = 8;

    public static IRuleBuilderOptions<T, string> ValidAccountEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Укажите адрес электронной почты.")
            .MaximumLength(MaxEmailLength)
            .WithMessage($"Адрес не длиннее {MaxEmailLength} символов.")
            .EmailAddress()
            .WithMessage("Адрес электронной почты указан неверно.");

    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        var rules = ruleBuilder
            .NotEmpty()
            .WithMessage("Укажите пароль.")
            .MinimumLength(PasswordPolicy.MinimumLength)
            .WithMessage($"Пароль не короче {PasswordPolicy.MinimumLength} символов.")
            .MaximumLength(PasswordPolicy.MaximumLength)
            .WithMessage($"Пароль не длиннее {PasswordPolicy.MaximumLength} символов.");

        if (PasswordPolicy.RequireDigit)
        {
            rules = rules.Must(ContainsDigit).WithMessage("В пароле нужна хотя бы одна цифра.");
        }

        if (PasswordPolicy.RequireLowercase)
        {
            rules = rules.Must(ContainsLowercase)
                .WithMessage("В пароле нужна хотя бы одна строчная буква.");
        }

        if (PasswordPolicy.RequireUppercase)
        {
            rules = rules.Must(ContainsUppercase)
                .WithMessage("В пароле нужна хотя бы одна заглавная буква.");
        }

        if (PasswordPolicy.RequireNonAlphanumeric)
        {
            rules = rules.Must(ContainsNonAlphanumeric)
                .WithMessage("В пароле нужен хотя бы один спецсимвол.");
        }

        return rules;
    }

    public static IRuleBuilderOptions<T, string> ValidTwoFactorCode<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Введите код из приложения-аутентификатора.")
            .Must(code => Digits(code).Length == TwoFactorCodeLength)
            .WithMessage($"Код состоит из {TwoFactorCodeLength} цифр.");

    /// <summary>Язык необязателен: пустое значение означает язык по умолчанию.</summary>
    public static IRuleBuilderOptions<T, string?> SupportedCulture<T>(
        this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
            .Must(culture => string.IsNullOrEmpty(culture) || SupportedCultures.IsSupported(culture))
            .WithMessage($"Поддерживаются языки: {string.Join(", ", SupportedCultures.All)}.");

    public static IRuleBuilderOptions<T, string> ValidRecoveryCode<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Введите код восстановления.")
            .MinimumLength(MinRecoveryCodeLength)
            .WithMessage("Код восстановления указан не полностью.");

    private static string Digits(string? value) =>
        value is null ? string.Empty : new string([.. value.Where(char.IsDigit)]);

    private static bool ContainsDigit(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);

    private static bool ContainsLowercase(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsLower);

    private static bool ContainsUppercase(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);

    private static bool ContainsNonAlphanumeric(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(character => !char.IsLetterOrDigit(character));
}