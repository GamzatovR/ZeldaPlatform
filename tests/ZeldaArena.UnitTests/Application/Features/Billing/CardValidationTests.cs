using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;
using ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

/// <summary>
/// Серверная половина двухуровневой валидации реквизитов (docs/SPEC.md §7.6, шаг 1;
/// §15). Она работает и при выключенном JavaScript — отдельный пункт чек-листа §19.
/// </summary>
public class CardValidationTests
{
    private readonly StartSubscriptionPaymentCommandValidator _validator = new();
    private readonly ConfirmPaymentCommandValidator _codeValidator = new();

    [Theory]
    [InlineData("4242424242424242")]
    [InlineData("4242 4242 4242 4242")]
    [InlineData("5555555555554444")]
    [InlineData("2200000000000004")]
    public void Real_card_numbers_pass_the_luhn_check(string number) =>
        CardNumber.PassesLuhn(number).ShouldBeTrue();

    [Theory]
    [InlineData("4242424242424243")]
    [InlineData("1234567890123456")]
    [InlineData("4242")]
    [InlineData("")]
    [InlineData(null)]
    public void Mistyped_numbers_fail_the_luhn_check(string? number) =>
        CardNumber.PassesLuhn(number).ShouldBeFalse();

    [Fact]
    public void A_valid_form_passes() =>
        _validator.Validate(Command()).IsValid.ShouldBeTrue();

    [Fact]
    public void A_mistyped_card_number_is_rejected() =>
        ShouldFail(Command() with { CardNumber = "4242424242424243" }, "CardNumber");

    /// <summary>
    /// Декабрь прошлого года проходит проверки года и месяца по отдельности,
    /// поэтому срок проверяется целиком.
    /// </summary>
    [Fact]
    public void An_expired_card_is_rejected() =>
        ShouldFail(Command() with { ExpiryMonth = 12, ExpiryYear = 2020 }, "ExpiryYear");

    [Fact]
    public void The_current_month_is_still_valid() =>
        _validator.Validate(Command() with
        {
            ExpiryMonth = DateTime.UtcNow.Month,
            ExpiryYear = DateTime.UtcNow.Year,
        }).IsValid.ShouldBeTrue();

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("abc")]
    public void A_malformed_cvv_is_rejected(string cvv) =>
        ShouldFail(Command() with { Cvv = cvv }, "Cvv");

    [Fact]
    public void A_malformed_receipt_address_is_rejected() =>
        ShouldFail(Command() with { ConfirmationEmail = "not-an-email" }, "ConfirmationEmail");

    [Fact]
    public void A_form_without_an_idempotency_key_is_rejected() =>
        ShouldFail(Command() with { IdempotencyKey = "" }, "IdempotencyKey");

    /// <summary>Заведомо негодный код не должен расходовать попытку (§7.6).</summary>
    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12345a")]
    [InlineData("")]
    public void A_malformed_confirmation_code_never_reaches_the_handler(string code) =>
        _codeValidator
            .Validate(new ConfirmPaymentCommand(Guid.CreateVersion7(), code))
            .IsValid
            .ShouldBeFalse();

    [Fact]
    public void A_six_digit_code_passes() =>
        _codeValidator
            .Validate(new ConfirmPaymentCommand(Guid.CreateVersion7(), "123456"))
            .IsValid
            .ShouldBeTrue();

    private void ShouldFail(StartSubscriptionPaymentCommand command, string property) =>
        _validator.Validate(command).Errors
            .ShouldContain(failure => failure.PropertyName == property);

    private static StartSubscriptionPaymentCommand Command() =>
        new(
            Guid.CreateVersion7(),
            "4242424242424242",
            12,
            2030,
            "123",
            "player@zeldaarena.test",
            "form-1");
}