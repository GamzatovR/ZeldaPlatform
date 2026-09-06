using System.Reflection;

namespace ZeldaArena.Domain;

/// <summary>
/// Якорь сборки Domain.
/// Нужен архитектурным тестам, чтобы получить сборку, не завися от конкретной сущности,
/// и регистрации обработчиков доменных событий по сборке начиная с Фазы 2.
/// </summary>
public static class DomainAssemblyReference
{
    public static Assembly Assembly => typeof(DomainAssemblyReference).Assembly;
}