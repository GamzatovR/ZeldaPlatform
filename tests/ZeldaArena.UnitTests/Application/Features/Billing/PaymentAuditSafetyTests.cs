using ZeldaArena.Application.Common.Behaviors;
using ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;
using ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

/// <summary>
/// Номер карты, CVV и код подтверждения не попадают в аудит и логи
/// (docs/SPEC.md §7.6, §13, §20 пункт 6).
///
/// Сам механизм маскирования проверяет <c>AuditBehaviorTests</c> на синтетической
/// команде, а список фрагментов — <c>SensitivePropertiesTests</c>. Здесь проверяется
/// третье, чего не видит ни тот, ни другой: что поля **настоящих** команд оплаты
/// названы так, что под этот список подошли. Переименование <c>CardNumber</c>
/// в <c>Pan</c> оставило бы оба прежних теста зелёными.
/// </summary>
public class PaymentAuditSafetyTests
{
    [Theory]
    [InlineData(nameof(StartSubscriptionPaymentCommand.CardNumber))]
    [InlineData(nameof(StartSubscriptionPaymentCommand.Cvv))]
    public void Card_fields_of_the_start_command_are_masked(string property) =>
        SensitiveProperties.IsSensitive(property).ShouldBeTrue(
            $"Поле {property} команды оплаты обязано попадать под маску (§7.6).");

    [Fact]
    public void The_confirmation_code_field_is_masked() =>
        SensitiveProperties
            .IsSensitive(nameof(ConfirmPaymentCommand.ConfirmationCode))
            .ShouldBeTrue();

    /// <summary>
    /// Обратная сторона: маскируется секрет, а не вся команда. По адресу чека
    /// и ключу идемпотентности платёж ищут в журнале, и они обязаны остаться читаемыми.
    /// </summary>
    [Theory]
    [InlineData(nameof(StartSubscriptionPaymentCommand.ConfirmationEmail))]
    [InlineData(nameof(StartSubscriptionPaymentCommand.IdempotencyKey))]
    [InlineData(nameof(StartSubscriptionPaymentCommand.PlanId))]
    public void Fields_needed_for_investigation_stay_readable(string property) =>
        SensitiveProperties.IsSensitive(property).ShouldBeFalse(
            $"Поле {property} нужно в журнале и маскироваться не должно.");

    /// <summary>
    /// Ни одно свойство команд оплаты не должно оказаться незамеченным секретом.
    /// Список ожидаемых имён обновляется вместе с командой — и это ровно тот момент,
    /// когда стоит задуматься, не секрет ли добавили.
    /// </summary>
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