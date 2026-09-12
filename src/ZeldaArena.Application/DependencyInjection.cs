using FluentValidation;

using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using ZeldaArena.Application.Common.Behaviors;
using ZeldaArena.Application.Features.Carts;
using ZeldaArena.Application.Features.Orders;
using ZeldaArena.Application.Features.Payments;

namespace ZeldaArena.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var assembly = ApplicationAssemblyReference.Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);

            // Порядок регистрации = порядок выполнения.
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(AuditBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Общие шаги сценариев, а не порты: живут в Application и зависят только от портов.
        services.AddScoped<PaymentInitiator>();
        services.AddScoped<CartLocator>();
        services.AddScoped<OrderNumberGenerator>();
        services.AddScoped<OrderCancellation>();

        // Своя конфигурация Mapster, а не TypeAdapterConfig.GlobalSettings: глобальная
        // статика протекала бы между тестами и между вызовами AddApplication.
        var mappingConfiguration = new TypeAdapterConfig();
        mappingConfiguration.Scan(assembly);

        services.AddSingleton(mappingConfiguration);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}