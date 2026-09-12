using ZeldaArena.Application.Common.Behaviors;
using ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;
using ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

public class PaymentAuditSafetyTests
{
    [Theory]
    [InlineData(nameof(StartSubscriptionPaymentCommand.CardNumber))]
    [InlineData(nameof(StartSubscriptionPaymentCommand.Cvv))]
    public void Card_fields_of_the_start_command_are_masked(string property) =>
        SensitiveProperties.IsSensitive(property).ShouldBeTrue(
            $"Поле {property} команды оплаты обязано попадать под маску.");

    [Fact]
    public void The_confirmation_code_field_is_masked() =>
        SensitiveProperties
            .IsSensitive(nameof(ConfirmPaymentCommand.ConfirmationCode))
            .ShouldBeTrue();

    [Theory]
    [InlineData(nameof(StartSubscriptionPaymentCommand.ConfirmationEmail))]
    [InlineData(nameof(StartSubscriptionPaymentCommand.IdempotencyKey))]
    [InlineData(nameof(StartSubscriptionPaymentCommand.PlanId))]
    public void Fields_needed_for_investigation_stay_readable(string property) =>
        SensitiveProperties.IsSensitive(property).ShouldBeFalse(
            $"Поле {property} нужно в журнале и маскироваться не должно.");

    [Fact]
    public void The_start_command_carries_no_unexpected_fields() =>
        typeof(StartSubscriptionPaymentCommand)
            .GetProperties()
            .Select(property => property.Name)
            .Where(name => name != "EqualityContract")
            .ShouldBe(
                [
                    nameof(StartSubscriptionPaymentCommand.PlanId),
                    nameof(StartSubscriptionPaymentCommand.CardNumber),
                    nameof(StartSubscriptionPaymentCommand.ExpiryMonth),
                    nameof(StartSubscriptionPaymentCommand.ExpiryYear),
                    nameof(StartSubscriptionPaymentCommand.Cvv),
                    nameof(StartSubscriptionPaymentCommand.ConfirmationEmail),
                    nameof(StartSubscriptionPaymentCommand.IdempotencyKey),
                    nameof(StartSubscriptionPaymentCommand.AuditEntityType),
                    nameof(StartSubscriptionPaymentCommand.AuditEntityId),
                ],
                ignoreOrder: true);
}