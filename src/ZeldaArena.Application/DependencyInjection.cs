using FluentValidation;

using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using ZeldaArena.Application.Common.Behaviors;
using ZeldaArena.Application.Features.Payments;

namespace ZeldaArena.Application;

/// <summary>
/// Composition root слоя сценариев. Web вызывает его в Program.cs; типы Application
/// в Web разрешены — правило 3 docs/SPEC.md §5.2 запрещает только типы Infrastructure.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var assembly = ApplicationAssemblyReference.Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);

            // Порядок регистрации = порядок выполнения. Логирование снаружи всего,
            // чтобы видеть и отвергнутые валидатором запросы; транзакция внутри всего,
            // чтобы аудит не оказался её частью (docs/adr/ADR-0004).
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(AuditBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Общие шаги сценариев, а не порты: живут в Application и зависят только от портов.
        services.AddScoped<PaymentInitiator>();

        // Своя конфигурация Mapster, а не TypeAdapterConfig.GlobalSettings: глобальная
        // статика протекала бы между тестами и между вызовами AddApplication.
        var mappingConfiguration = new TypeAdapterConfig();
        mappingConfiguration.Scan(assembly);

        services.AddSingleton(mappingConfiguration);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}