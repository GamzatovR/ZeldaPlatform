namespace ZeldaArena.Web.Areas.Api.Models;

public sealed record CartOperationResponse(int ItemCount, string MiniCartLabel, string Message);