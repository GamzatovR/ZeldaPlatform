using ZeldaArena.Application;
using ZeldaArena.Domain;

namespace ZeldaArena.UnitTests;

public class SolutionSmokeTests
{
    [Fact]
    public void Core_assemblies_are_loadable()
    {
        DomainAssemblyReference.Assembly.GetName().Name.ShouldBe("ZeldaArena.Domain");
        ApplicationAssemblyReference.Assembly.GetName().Name.ShouldBe("ZeldaArena.Application");
    }
}