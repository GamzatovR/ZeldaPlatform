using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Платёжный провайдер (docs/SPEC.md §7.6). Реализация — <c>FakePaymentGateway</c>
/// в Фазе 4: оплата мнимая, авторизация сводится к проверке номера по алгоритму Луна
/// и определению бренда по BIN.
///
/// <see cref="Key"/> существует ради EP-6 (§5.4): новый провайдер добавляется классом
/// и регистрацией, резолвер выбирает его по строковому ключу из конфигурации.
///
/// Наружу метод отдаёт только <see cref="CardAuthorization"/> — бренд и последние четыре
/// цифры. Полный номер и CVV остаются внутри вызова: их нельзя сохранять, логировать
/// и возвращать клиенту (§7.6, §20 пункт 6). Настоящий приём платежей потребовал бы
/// PCI DSS и токенизации на стороне провайдера — этот порт для того и заведён.
/// </summary>
public interface IPaymentGateway
{
    string Key { get; }

    Task<Result<CardAuthorization>> AuthorizeAsync(
        CardPaymentRequest request,
        CancellationToken cancellationToken = default);
}