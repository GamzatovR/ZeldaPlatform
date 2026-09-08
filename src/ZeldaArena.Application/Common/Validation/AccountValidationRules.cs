using FluentValidation;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Common.Validation;

/// <summary>
/// Правила, общие для всех сценариев аккаунта: адрес, пароль, язык интерфейса.
///
/// Вынесены в одно место, потому что пароль проверяется в четырёх командах —
/// регистрация, сброс, смена, сид администратора. Разложенные по валидаторам копии
/// разъехались бы при первом же изменении требований §8.2.
/// </summary>
public static class AccountValidationRules
{
    public const int MaxEmailLength = 256;

    public static IRuleBuilderOptions<T, string> ValidAccountEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Укажите адрес электронной почты.")
            .MaximumLength(MaxEmailLength)
            .WithMessage($"Адрес не длиннее {MaxEmailLength} символов.")
            .EmailAddress()
            .WithMessage("Адрес электронной почты указан неверно.");

    /// <summary>
    /// Требования §8.2 проверяются по одному, чтобы сообщение говорило, чего именно
    /// не хватает. Ответ «пароль не подходит» без объяснения — это несколько попыток
    /// вслепую вместо одной осмысленной.
    /// </summary>
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

    /// <summary>Язык необязателен: пустое значение означает язык по умолчанию.</summary>
    public static IRuleBuilderOptions<T, string?> SupportedCulture<T>(
        this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
            .Must(culture => string.IsNullOrEmpty(culture) || SupportedCultures.IsSupported(culture))
            .WithMessage($"Поддерживаются языки: {string.Join(", ", SupportedCultures.All)}.");

    private static bool ContainsDigit(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);

    private static bool ContainsLowercase(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsLower);

    private static bool ContainsUppercase(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);

    private static bool ContainsNonAlphanumeric(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(character => !char.IsLetterOrDigit(character));
}