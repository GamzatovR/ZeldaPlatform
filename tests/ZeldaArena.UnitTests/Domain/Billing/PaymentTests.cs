using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.Billing;

public class PaymentTests
{
    private const string CodeHash = "e3b0c44298fc1c149afbf4c8996fb924";
    private const string WrongHash = "ffffffffffffffffffffffffffffffff";

    private static readonly DateTimeOffset Now = new(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Payment_stores_only_the_last_four_digits_and_the_code_hash()
    {
        var payment = StartPayment();

        payment.CardLast4.ShouldBe("4242");
        payment.CardBrand.ShouldBe("Visa");
        payment.ConfirmationCodeHash.ShouldBe(CodeHash);
        payment.ConfirmationAttemptsLeft.ShouldBe(Payment.MaxAttempts);
        payment.Status.ShouldBe(PaymentStatus.Pending);

        // Полей под полный номер карты и CVV в сущности нет вовсе (docs/SPEC.md §7.6).
        typeof(Payment).GetProperties()
            .Select(property => property.Name)
            .ShouldNotContain(name => name.Contains("Cvv", StringComparison.OrdinalIgnoreCase)
                || name.Equals("CardNumber", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("424")]
    [InlineData("42424")]
    [InlineData("42a4")]
    public void Card_last4_must_be_four_digits(string cardLast4)
    {
        Should.Throw<InvariantViolationException>(() => StartPayment(cardLast4: cardLast4))
            .Code.ShouldBe("payment.invalid_card_last4");
    }

    [Fact]
    public void Subscription_payment_cannot_reference_an_order()
    {
        Should.Throw<InvariantViolationException>(() => Payment.Start(
                Guid.CreateVersion7(),
                PaymentPurpose.Subscription,
                new Money(299m, "RUB"),
                "4242",
                "Visa",
                "player@example.com",
                CodeHash,
                Now.AddMinutes(10),
                "idem-1",
                orderId: Guid.CreateVersion7()))
            .Code.ShouldBe("payment.purpose_mismatch");
    }

    [Fact]
    public void Correct_code_confirms_the_payment_and_raises_an_event()
    {
        var payment = StartPayment();

        var result = payment.Confirm(CodeHash, Now.AddMinutes(1));

        result.ShouldBe(PaymentConfirmationResult.Succeeded);
        payment.Status.ShouldBe(PaymentStatus.Succeeded);
        payment.PaidAt.ShouldBe(Now.AddMinutes(1));
        payment.DomainEvents.OfType<PaymentConfirmedEvent>().ShouldHaveSingleItem()
            .Amount.ShouldBe(299m);
    }

    [Fact]
    public void Wrong_code_spends_one_attempt()
    {
        var payment = StartPayment();

        var result = payment.Confirm(WrongHash, Now.AddMinutes(1));

        result.ShouldBe(PaymentConfirmationResult.WrongCode);
        payment.ConfirmationAttemptsLeft.ShouldBe(Payment.MaxAttempts - 1);
        payment.Status.ShouldBe(PaymentStatus.Pending);
    }

    [Fact]
    public void Running_out_of_attempts_fails_the_payment()
    {
        var payment = StartPayment();

        for (var attempt = 0; attempt < Payment.MaxAttempts - 1; attempt++)
        {
            payment.Confirm(WrongHash, Now.AddMinutes(1)).ShouldBe(PaymentConfirmationResult.WrongCode);
        }

        payment.Confirm(WrongHash, Now.AddMinutes(1)).ShouldBe(PaymentConfirmationResult.NoAttemptsLeft);
        payment.Status.ShouldBe(PaymentStatus.Failed);
        payment.FailureReason.ShouldBe("payment.no_attempts_left");
    }

    [Fact]
    public void Expired_code_fails_the_payment_even_if_it_is_correct()
    {
        var payment = StartPayment();

        var result = payment.Confirm(CodeHash, Now.AddMinutes(11));

        result.ShouldBe(PaymentConfirmationResult.Expired);
        payment.Status.ShouldBe(PaymentStatus.Failed);
        payment.FailureReason.ShouldBe("payment.code_expired");
    }

    [Fact]
    public void Confirming_twice_is_idempotent_from_the_entity_point_of_view()
    {
        var payment = StartPayment();
        payment.Confirm(CodeHash, Now.AddMinutes(1));

        payment.Confirm(CodeHash, Now.AddMinutes(2)).ShouldBe(PaymentConfirmationResult.AlreadyProcessed);
        payment.DomainEvents.OfType<PaymentConfirmedEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Reissued_code_resets_attempts_and_deadline()
    {
        var payment = StartPayment();
        payment.Confirm(WrongHash, Now.AddMinutes(1));

        payment.ReissueCode("aaaa1111bbbb2222cccc3333dddd4444", Now.AddMinutes(20));

        payment.ConfirmationAttemptsLeft.ShouldBe(Payment.MaxAttempts);
        payment.ConfirmationExpiresAt.ShouldBe(Now.AddMinutes(20));
        payment.Confirm("aaaa1111bbbb2222cccc3333dddd4444", Now.AddMinutes(15))
            .ShouldBe(PaymentConfirmationResult.Succeeded);
    }

    [Fact]
    public void Successful_payment_cannot_be_marked_as_failed()
    {
        var payment = StartPayment();
        payment.Confirm(CodeHash, Now.AddMinutes(1));

        Should.Throw<InvariantViolationException>(() => payment.Fail("whatever"))
            .Code.ShouldBe("payment.already_succeeded");
    }

    private static Payment StartPayment(string cardLast4 = "4242") =>
        Payment.Start(
            Guid.CreateVersion7(),
            PaymentPurpose.Subscription,
            new Money(299m, "RUB"),
            cardLast4,
            "Visa",
            "player@example.com",
            CodeHash,
            Now.AddMinutes(10),
            "idem-1");
}