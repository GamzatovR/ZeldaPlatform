using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Infrastructure.BackgroundJobs;
using ZeldaArena.Infrastructure.Common;
using ZeldaArena.Infrastructure.Email;
using ZeldaArena.Infrastructure.Files;
using ZeldaArena.Infrastructure.Identity;
using ZeldaArena.Infrastructure.Logging;
using ZeldaArena.Infrastructure.Payments;
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

        // Кэш прав на платные функции: TTL пять минут (docs/SPEC.md §7.3).
        services.AddMemoryCache();

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

        // Перец обязателен и проверяется при старте: без него шестизначный код
        // защищён одним лишь SHA-256, а это миллион вариантов для перебора
        // по дампу базы (docs/SPEC.md §7.6). Лучше не подняться, чем тихо
        // работать с ослабленным хешем.
        services.AddOptions<ConfirmationCodeOptions>()
            .Bind(configuration.GetSection(ConfirmationCodeOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Pepper),
                "Не задан Payments:ConfirmationCode:Pepper — секрет для хеша кода подтверждения.")
            .ValidateOnStart();

        // Порты Application → реализации Infrastructure. Дальше о существовании
        // этих классов не знает никто.
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IQueryExecutor, EfQueryExecutor>();

        // Кэш и его сброс — одно состояние, поэтому один синглтон на два входа:
        // читает его EntitlementService, сбрасывают обработчики событий и админка.
        services.AddSingleton<EntitlementCache>();
        services.AddSingleton<IEntitlementCacheInvalidator>(provider =>
            provider.GetRequiredService<EntitlementCache>());
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUserAccountService, IdentityUserAccountService>();
        services.AddScoped<IEntitlementService, EntitlementService>();
        services.AddScoped<ISignInService, IdentitySignInService>();
        services.AddScoped<ITwoFactorService, IdentityTwoFactorService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        // Мнимая оплата (§7.6). Провайдер меняется одной строкой — ради этого
        // у порта и есть Key (EP-6).
        services.AddSingleton<IPaymentGateway, FakePaymentGateway>();
        services.AddSingleton<IConfirmationCodeProtector, ConfirmationCodeProtector>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        services.AddScoped<INewsRepository, EfNewsRepository>();

        // Загруженные логотипы и аватары (docs/SPEC.md §15): вне wwwroot, под GUID-именем.
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        // Фаза 10 заменит эту строку на MongoAuditLogWriter (docs/SPEC.md §12, §13).
        services.AddScoped<IAuditLogWriter, LoggerAuditLogWriter>();

        // Раз в час помечает истёкшие подписки (docs/SPEC.md §7.5, п. 4).
        services.AddHostedService<SubscriptionExpirationService>();

        services.AddScoped<IdentitySeeder>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}