using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

/// <summary>
/// Дашборд админки — страница <c>/admin</c> (docs/SPEC.md §9.4, п. 1): число пользователей,
/// активных подписок, выручка, ближайшие матчи.
///
/// Последние события аудита из того же пункта появятся в Фазе 10 вместе с хранилищем
/// аудита в MongoDB: сейчас журнал пишется только в лог приложения, и читать его
/// дашборду неоткуда. Блок-заглушка, изображающий данные, хуже отсутствующего блока.
/// </summary>
/// <param name="MatchCount">Сколько ближайших матчей показать.</param>
/// <param name="OrderCount">Сколько последних заказов показать.</param>
public sealed record GetAdminDashboardQuery(int MatchCount = 5, int OrderCount = 5)
    : IQuery<AdminDashboardDto>;