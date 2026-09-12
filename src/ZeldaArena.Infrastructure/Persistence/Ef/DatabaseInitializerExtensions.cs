using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ZeldaArena.Infrastructure.Identity;
using ZeldaArena.Infrastructure.Persistence.Ef.Seed;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

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

        // Роли и учётные записи идут первыми: новость требует автора с внешним ключом
        // на AspNetUsers, поэтому предметный сид без них не пройдёт.
        var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        await identitySeeder.SeedAsync(cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}