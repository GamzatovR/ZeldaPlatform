using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ZeldaArena.Infrastructure.Persistence.Ef.Seed;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

/// <summary>
/// Применение миграций и сид при старте. Вызывается из Program.cs — единственного
/// места в Web, где допустимы типы Infrastructure (docs/SPEC.md §5.2, правило 3).
/// </summary>
public static class DatabaseInitializerExtensions
{
    public static async Task MigrateAndSeedAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync(cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}