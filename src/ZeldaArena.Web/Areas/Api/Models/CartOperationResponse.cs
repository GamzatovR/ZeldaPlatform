namespace ZeldaArena.Web.Areas.Api.Models;

/// <summary>
/// Ответ операции с корзиной: новое число товаров для мини-корзины в шапке, подпись
/// к нему для скринридера и сообщение для тоста — всё уже на языке пользователя.
/// </summary>
public sealed record CartOperationResponse(int ItemCount, string MiniCartLabel, string Message);
