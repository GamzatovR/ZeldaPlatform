namespace ZeldaArena.Web.Areas.Api.Models;

/// <summary>
/// Операция удалась, страница остаётся прежней: клиент показывает сообщение тостом
/// и перерисовывает то, что изменилось (admin-actions.js).
/// </summary>
public sealed record MessageResponse(string Message);