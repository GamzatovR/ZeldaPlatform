using ZeldaArena.Application;
using ZeldaArena.Domain;

namespace ZeldaArena.UnitTests;

/// <summary>
/// Заглушка Фазы 0: проект тестов должен собираться и запускаться в CI ещё до появления домена.
/// Наполняется тестами сущностей в Фазе 1.
/// </summary>
public class SolutionSmokeTests
{
    [Fact]
    public void Core_assemblies_are_loadable()
    {
        DomainAssemblyReference.Assembly.GetName().Name.ShouldBe("ZeldaArena.Domain");
        ApplicationAssemblyReference.Assembly.GetName().Name.ShouldBe("ZeldaArena.Application");
    }
}