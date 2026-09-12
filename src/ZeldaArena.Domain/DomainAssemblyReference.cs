using System.Reflection;

namespace ZeldaArena.Domain;

/// <summary>Якорь сборки Domain.</summary>
public static class DomainAssemblyReference
{
    public static Assembly Assembly => typeof(DomainAssemblyReference).Assembly;
}