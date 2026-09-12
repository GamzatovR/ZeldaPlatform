using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ZeldaArena.Infrastructure.BackgroundJobs;

public abstract class PeriodicCommandService(
    IServiceScopeFactory scopeFactory,
    ILogger logger)
    : BackgroundService
{
    /// <summary>Как часто запускать команду.</summary>
    protected abstract TimeSpan Interval { get; }

    protected virtual TimeSpan StartupDelay => TimeSpan.FromMinutes(1);

    /// <summary>Что писать в журнал, если запуск не удался.</summary>
    protected abstract string FailureMessage { get; }

    protected abstract IBaseRequest CreateCommand();

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
            await RunAsync(stoppingToken).ConfigureAwait(false);
        }
        while (await WaitAsync(timer, stoppingToken).ConfigureAwait(false));
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            await sender.Send((object)CreateCommand(), stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Приложение останавливается — это не сбой.
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "{FailureMessage}", FailureMessage);
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