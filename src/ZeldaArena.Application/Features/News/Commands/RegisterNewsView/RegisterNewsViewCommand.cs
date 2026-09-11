using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.News.Commands.RegisterNewsView;

/// <summary>
/// Счётчик просмотров новости (<c>NewsArticles.ViewCount</c>, docs/SPEC.md §6).
/// Не аудируется: просмотр — не действие над сущностью, а журнал действий
/// должен читаться (docs/adr/ADR-0004).
/// </summary>
public sealed record RegisterNewsViewCommand(Guid ArticleId) : ICommand;