using System.Reflection;

namespace ZeldaArena.Application;

public static class ApplicationAssemblyReference
{
    public static Assembly Assembly => typeof(ApplicationAssemblyReference).Assembly;
}