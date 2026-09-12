using FluentValidation;

using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.ArchitectureTests;

public class ApplicationModelSentinelTests
{
    /// <summary>Четыре behavior: Validation, Audit, Logging, Transaction.</summary>
    private const int ExpectedBehaviorCount = 4;

    /// <summary>Порты; реализуются в разных фазах, но объявлены все сразу.</summary>
    private const int ExpectedPortCount = 14;

    [Fact]
    public void Application_assembly_should_contain_requests()
    {
        Requests().ShouldNotBeEmpty(
            "В Application нет ни одного IRequest — слой сценариев пуст, и проверки "
            + "правила зависимостей ничего не проверяют.");
    }

    [Fact]
    public void Application_assembly_should_contain_handlers()
    {
        Handlers().ShouldNotBeEmpty("В Application нет ни одного хендлера.");
    }

    [Fact]
    public void Application_assembly_should_contain_the_whole_pipeline()
    {
        var behaviors = ApplicationTypes()
            .Where(type => Implements(type, typeof(IPipelineBehavior<,>)))
            .ToArray();

        behaviors.Length.ShouldBeGreaterThanOrEqualTo(
            ExpectedBehaviorCount,
            $"Конвейер неполон: behaviors найдено {behaviors.Length}, "
            + $"ожидалось не меньше {ExpectedBehaviorCount}.");
    }

    [Fact]
    public void Application_assembly_should_contain_validators()
    {
        ApplicationTypes()
            .Where(type => IsSubclassOfRawGeneric(typeof(AbstractValidator<>), type))
            .ShouldNotBeEmpty("Ни одного валидатора FluentValidation не найдено.");
    }

    [Fact]
    public void Application_should_declare_all_ports()
    {
        var ports = ArchitectureFixture.Application.GetTypes()
            .Where(type => type.IsInterface)
            .Where(type => string.Equals(
                type.Namespace,
                typeof(IUnitOfWork).Namespace,
                StringComparison.Ordinal))
            .ToArray();

        ports.Length.ShouldBeGreaterThanOrEqualTo(
            ExpectedPortCount,
            $"Портов объявлено {ports.Length}, ожидалось не меньше {ExpectedPortCount}.");
    }

    [Fact]
    public void Every_request_should_have_a_handler()
    {
        var handled = Handlers()
            .SelectMany(handler => handler.GetInterfaces())
            .Where(contract => contract.IsGenericType
                && contract.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            .Select(contract => contract.GetGenericArguments()[0])
            .ToHashSet();

        var orphans = Requests()
            .Where(request => !handled.Contains(request))
            .Select(request => request.FullName ?? request.Name)
            .ToArray();

        orphans.ShouldBeEmpty($"У этих запросов нет хендлера: {string.Join(", ", orphans)}");
    }

    [Fact]
    public void Every_command_should_be_marked_for_the_transaction_behavior()
    {
        var unmarked = Requests()
            .Where(type => type.Name.EndsWith("Command", StringComparison.Ordinal))
            .Where(type => !typeof(ICommandBase).IsAssignableFrom(type))
            .Select(type => type.FullName ?? type.Name)
            .ToArray();

        unmarked.ShouldBeEmpty(
            $"Эти команды не реализуют ICommand: {string.Join(", ", unmarked)}");
    }

    private static Type[] Requests() =>
        [.. ApplicationTypes().Where(type => Implements(type, typeof(IRequest<>)))];

    private static Type[] Handlers() =>
        [.. ApplicationTypes().Where(type => Implements(type, typeof(IRequestHandler<,>)))];

    private static IEnumerable<Type> ApplicationTypes() =>
        ArchitectureFixture.Application.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => !ArchitectureFixture.IsCompilerGenerated(type));

    private static bool Implements(Type type, Type openGenericInterface) =>
        Array.Exists(
            type.GetInterfaces(),
            contract => contract.IsGenericType
                && contract.GetGenericTypeDefinition() == openGenericInterface);

    private static bool IsSubclassOfRawGeneric(Type openGeneric, Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == openGeneric)
            {
                return true;
            }
        }

        return false;
    }
}