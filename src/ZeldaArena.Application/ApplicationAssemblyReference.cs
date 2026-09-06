using System.Reflection;

namespace ZeldaArena.Application;

/// <summary>
/// Якорь сборки Application.
/// Используется архитектурными тестами и сканированием сборки при регистрации
/// MediatR-хендлеров, валидаторов и профилей маппинга (Фаза 2).
/// </summary>
public static class ApplicationAssemblyReference
{
    public static Assembly Assembly => typeof(ApplicationAssemblyReference).Assembly;
}
