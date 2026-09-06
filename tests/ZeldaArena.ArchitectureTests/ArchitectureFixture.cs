using System.Reflection;
using System.Runtime.CompilerServices;

using ZeldaArena.Application;
using ZeldaArena.Domain;

namespace ZeldaArena.ArchitectureTests;

/// <summary>
/// Сборки слоёв и общие помощники для проверок правила зависимостей (docs/SPEC.md §5.2).
/// </summary>
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

    /// <summary>
    /// Имена сборок, на которые ссылается указанная сборка. Проверка по ссылкам дополняет
    /// проверку по типам: она срабатывает даже тогда, когда запрещённый пакет уже подключён,
    /// но им ещё никто не воспользовался.
    /// </summary>
    public static IReadOnlyCollection<string> ReferencedAssemblyNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

    public static bool IsCompilerGenerated(Type type) =>
        type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
        || type.Name.StartsWith('<');

    /// <summary>
    /// Имя корневого типа: для вложенных и сгенерированных компилятором типов
    /// (замыкания при точке входа) возвращает объемлющий тип.
    /// </summary>
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