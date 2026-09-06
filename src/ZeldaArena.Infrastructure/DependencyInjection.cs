using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
