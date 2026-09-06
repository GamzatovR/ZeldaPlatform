using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ZeldaArena.Infrastructure.Persistence.Ef;
using ZeldaArena.Infrastructure.Persistence.Ef.Interceptors;

namespace ZeldaArena.Infrastructure;

/// <summary>
/// Composition root инфраструктуры: единственная точка, где Web видит типы этого проекта
/// (docs/SPEC.md §5.2, правило 3). Реализации портов подключаются здесь начиная с Фазы 1.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));

            options.AddInterceptors(provider.GetRequiredService<AuditableEntityInterceptor>());
        });

        return services;
    }
}