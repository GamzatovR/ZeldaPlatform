using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using ZeldaArena.Application.Common.Events;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Interceptors;

/// <summary>
/// Рассылает доменные события после сохранения изменений.
///
/// Почему после, а не до: до сохранения у только что созданной сущности ещё нет
/// гарантии, что запись вообще ляжет в базу, и обработчик рассылал бы новости
/// о несуществующем факте.
///
/// Почему интерсептор, а не хендлер: собрать события можно только через change tracker,
/// а он — деталь EF Core, которой в Application быть не может. Хендлер вызывает
/// метод сущности и о рассылке не думает (docs/SPEC.md §5.5, SRP).
///
/// Накопитель чистится до вызова обработчиков: обработчик вправе снова сохранить
/// контекст, и без очистки те же события ушли бы по второму кругу.
/// </summary>
public sealed class DispatchDomainEventsInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        await DispatchAsync(eventData.Context, cancellationToken).ConfigureAwait(false);

        return await base.SavedChangesAsync(eventData, result, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Синхронное сохранение в приложении не используется (CLAUDE.md: асинхронность
    /// везде, где есть I/O), но молча терять события, если такой вызов однажды
    /// появится, нельзя — это была бы неотлаживаемая пропажа.
    /// </summary>
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        DispatchAsync(eventData.Context, CancellationToken.None).AsTask().GetAwaiter().GetResult();

        return base.SavedChanges(eventData, result);
    }

    private async ValueTask DispatchAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null)
        {
            return;
        }

        var sources = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToArray();

        if (sources.Length == 0)
        {
            return;
        }

        var domainEvents = sources.SelectMany(source => source.DomainEvents).ToArray();

        foreach (var source in sources)
        {
            source.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await publisher
                .Publish(DomainEventNotificationFactory.Create(domainEvent), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}