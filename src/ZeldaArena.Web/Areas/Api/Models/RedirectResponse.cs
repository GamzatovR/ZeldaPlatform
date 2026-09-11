namespace ZeldaArena.Web.Areas.Api.Models;

/// <summary>
/// Операция удалась, следующий шаг — другая страница: ввод кода, успех оплаты, заказ.
/// Адрес решает сервер, клиент только переходит по нему.
/// </summary>
public sealed record RedirectResponse(string RedirectUrl);