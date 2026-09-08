using ZeldaArena.Application.Features.Account.Commands.RegisterUser;
using ZeldaArena.Application.Features.Account.Commands.SignIn;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.UnitTests.Application.Features.Account;

/// <summary>
/// Серверный уровень двухуровневой валидации (docs/SPEC.md §15). Проверяется именно он,
/// потому что клиентский можно обойти, выключив JavaScript, — отдельный пункт §19.
/// </summary>
public class AccountValidatorTests
{
    private static readonly RegisterUserCommandValidator Register = new();
    private static readonly SignInCommandValidator SignIn = new();

    [Fact]
    public void Well_formed_registration_passes() =>
        Register.Validate(Registration()).IsValid.ShouldBeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("не-адрес")]
    [InlineData("@zeldaarena.test")]
    public void Malformed_email_is_rejected(string email) =>
        Register.Validate(Registration(email: email)).IsValid.ShouldBeFalse();

    /// <summary>Требования §8.2: не короче десяти символов, цифра, регистры и спецсимвол.</summary>
    [Theory]
    [InlineData("Short-1a")]
    [InlineData("password-1234")]
    [InlineData("PASSWORD-1234")]
    [InlineData("PasswordNoDigit-")]
    [InlineData("Password12345")]
    public void Weak_password_is_rejected(string password) =>
        Register.Validate(Registration(password: password)).IsValid.ShouldBeFalse();

    [Fact]
    public void Password_at_the_minimum_length_passes() =>
        Register.Validate(Registration(password: "Passw0rd-!")).IsValid.ShouldBeTrue();

    [Fact]
    public void Password_length_is_capped() =>
        Register.Validate(Registration(
            password: "Aa1-" + new string('z', PasswordPolicy.MaximumLength)))
            .IsValid.ShouldBeFalse();

    [Fact]
    public void Too_long_display_name_is_rejected() =>
        Register.Validate(Registration(
            displayName: new string('z', RegisterUserCommandValidator.MaxDisplayNameLength + 1)))
            .IsValid.ShouldBeFalse();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(SupportedCultures.Russian)]
    [InlineData(SupportedCultures.English)]
    public void Supported_or_absent_culture_passes(string? culture) =>
        Register.Validate(Registration(culture: culture)).IsValid.ShouldBeTrue();

    [Fact]
    public void Unsupported_culture_is_rejected() =>
        Register.Validate(Registration(culture: "de")).IsValid.ShouldBeFalse();

    /// <summary>
    /// На входе требования к паролю не проверяются: пароль мог быть заведён до того,
    /// как правила ужесточили, и такому пользователю нужно дать войти и сменить его.
    /// </summary>
    [Fact]
    public void Sign_in_accepts_a_password_that_registration_would_reject() =>
        SignIn.Validate(new SignInCommand("player@zeldaarena.test", "old", RememberMe: false))
            .IsValid.ShouldBeTrue();

    [Fact]
    public void Sign_in_without_a_password_is_rejected() =>
        SignIn.Validate(new SignInCommand("player@zeldaarena.test", string.Empty, RememberMe: false))
            .IsValid.ShouldBeFalse();

    private static RegisterUserCommand Registration(
        string email = "player@zeldaarena.test",
        string password = "Password-1234",
        string? displayName = "Ganondorf",
        string? culture = SupportedCultures.Russian) =>
        new(email, password, displayName, culture);
}