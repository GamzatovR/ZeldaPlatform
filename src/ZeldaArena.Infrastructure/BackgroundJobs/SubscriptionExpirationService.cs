using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Features.Subscriptions.Commands.ExpireDueSubscriptions;

namespace ZeldaArena.Infrastructure.BackgroundJobs;

/// <summary>
/// Раз в час помечает истёкшие подписки (docs/SPEC.md §7.5, п. 4).
///
/// Служба сама ничего не решает: вся работа в команде, которая проходит обычный
/// конвейер с транзакцией, аудитом и журналом. Здесь только расписание и область
/// зависимостей — <c>ISender</c> и <c>AppDbContext</c> живут ограниченное время,
/// а фоновая служба существует всё время работы приложения, поэтому scope
/// создаётся на каждый запуск.
///
/// Исключение гасится намеренно: недоступная на минуту база не должна ронять
/// приложение целиком. Следующий запуск повторит работу, а незамеченным сбой
/// не останется — он попадёт в журнал.
/// </summary>
public sealed class SubscriptionExpirationService(
    IServiceScopeFactory scopeFactory,
    ILogger<SubscriptionExpirationService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    /// <summary>
    /// Первый проход отложен: на старте приложение занято миграциями и сидом,
    /// а часовой точности этой задаче хватает с запасом.
    /// </summary>
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(Interval);

        do
        {
            await ExpireAsync(stoppingToken).ConfigureAwait(false);
        }
        while (await WaitAsync(timer, stoppingToken).ConfigureAwait(false));
    }

    private async Task ExpireAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            await sender.Send(new ExpireDueSubscriptionsCommand(), stoppingToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Приложение останавливается — это не сбой.
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Не удалось обработать истёкшие подписки.");
        }
    }

    private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}