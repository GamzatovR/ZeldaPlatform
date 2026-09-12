using System.Reflection;
using System.Runtime.CompilerServices;

using ZeldaArena.Application;
using ZeldaArena.Domain;

namespace ZeldaArena.ArchitectureTests;

internal static class ArchitectureFixture
{
    public const string DomainNamespace = "ZeldaArena.Domain";
    public const string ApplicationNamespace = "ZeldaArena.Application";
    public const string InfrastructureNamespace = "ZeldaArena.Infrastructure";
    public const string WebNamespace = "ZeldaArena.Web";

    public const string EntityFrameworkNamespace = "Microsoft.EntityFrameworkCore";
    public const string AspNetCoreNamespace = "Microsoft.AspNetCore";

    public static Assembly Domain => DomainAssemblyReference.Assembly;

    public static Assembly Application => ApplicationAssemblyReference.Assembly;

    public static Assembly Infrastructure => typeof(Infrastructure.DependencyInjection).Assembly;

    public static Assembly Web => typeof(Program).Assembly;

    public static IReadOnlyCollection<string> ReferencedAssemblyNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

    public static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ZeldaArena.sln")))
        {
            directory = directory.Parent;
        }

        directory.ShouldNotBeNull("Не найден корень решения: ZeldaArena.sln выше каталога сборки нет.");

        return directory.FullName;
    }

    public static bool IsCompilerGenerated(Type type) =>
        type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
        || type.Name.StartsWith('<');

    public static string RootTypeName(Type type)
    {
        var root = type;
        while (root.DeclaringType is not null)
        {
            root = root.DeclaringType;
        }

        return root.Name;
    }
}