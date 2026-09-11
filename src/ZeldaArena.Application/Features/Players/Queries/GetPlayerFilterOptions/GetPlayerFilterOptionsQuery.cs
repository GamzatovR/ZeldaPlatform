using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerFilterOptions;

/// <summary>
/// Значения для выпадающих списков фильтра игроков: страны, которые реально встречаются,
/// и команды. Список стран из базы, а не весь ISO 3166: пункт, по которому
/// заведомо ничего не найдётся, — ловушка, а не выбор.
/// </summary>
public sealed record GetPlayerFilterOptionsQuery : IQuery<PlayerFilterOptionsDto>;