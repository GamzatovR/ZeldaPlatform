using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Infrastructure.Common;
using ZeldaArena.Infrastructure.Email;
using ZeldaArena.Infrastructure.Identity;
using ZeldaArena.Infrastructure.Logging;
using ZeldaArena.Infrastructure.Persistence.Ef;
using ZeldaArena.Infrastructure.Persistence.Ef.Interceptors;
using ZeldaArena.Infrastructure.Persistence.Ef.Seed;

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
        services.AddHttpContextAccessor();

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));

            // Порядок важен: отметки времени проставляются до сохранения,
            // доменные события рассылаются после него.
            options.AddInterceptors(
                provider.GetRequiredService<AuditableEntityInterceptor>(),
                provider.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        // Identity подключается до портов: реализации ниже опираются на UserManager
        // и SignInManager, зарегистрированные здесь (docs/SPEC.md §8).
        services.AddPlatformIdentity(configuration);

        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<SeedAccountsOptions>(
            configuration.GetSection(SeedAccountsOptions.SectionName));

        // Порты Application → реализации Infrastructure. Дальше о существовании
        // этих классов не знает никто.
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IQueryExecutor, EfQueryExecutor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUserAccountService, IdentityUserAccountService>();
        services.AddScoped<ISignInService, IdentitySignInService>();
        services.AddScoped<ITwoFactorService, IdentityTwoFactorService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        services.AddScoped<INewsRepository, EfNewsRepository>();

        // Фаза 10 заменит эту строку на MongoAuditLogWriter (docs/SPEC.md §12, §13).
        services.AddScoped<IAuditLogWriter, LoggerAuditLogWriter>();

        services.AddScoped<IdentitySeeder>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}