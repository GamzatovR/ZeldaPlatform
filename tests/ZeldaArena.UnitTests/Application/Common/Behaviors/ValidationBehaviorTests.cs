using FluentValidation;

using ZeldaArena.Application.Common.Behaviors;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.UnitTests.Application.Common.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Invalid_request_never_reaches_the_handler()
    {
        var handlerCalled = false;
        var behavior = new ValidationBehavior<TestCommand, Result>([new NameValidator()]);

        var exception = await Should.ThrowAsync<ValidationException>(() => behavior.Handle(
            new TestCommand(string.Empty, 10),
            _ =>
            {
                handlerCalled = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None));

        handlerCalled.ShouldBeFalse();
        exception.Errors.ShouldContain(failure => failure.ErrorMessage == "Имя обязательно.");
    }

    [Fact]
    public async Task Valid_request_reaches_the_handler()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([new NameValidator()]);

        var result = await behavior.Handle(
            new TestCommand("Zelda Cup", 10),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Request_without_validators_passes_through()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([]);

        var result = await behavior.Handle(
            new TestCommand(string.Empty, -1),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task All_validators_run_and_all_errors_are_collected()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>(
            [new NameValidator(), new AmountValidator()]);

        var exception = await Should.ThrowAsync<ValidationException>(() => behavior.Handle(
            new TestCommand(string.Empty, -5),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None));

        exception.Errors.Count().ShouldBe(2);
        exception.Errors.ShouldContain(failure => failure.ErrorMessage == "Имя обязательно.");
        exception.Errors.ShouldContain(failure => failure.ErrorMessage == "Сумма неотрицательна.");
    }

    private sealed record TestCommand(string Name, int Amount) : ICommand;

    private sealed class NameValidator : AbstractValidator<TestCommand>
    {
        public NameValidator() =>
            RuleFor(command => command.Name).NotEmpty().WithMessage("Имя обязательно.");
    }

    private sealed class AmountValidator : AbstractValidator<TestCommand>
    {
        public AmountValidator() =>
            RuleFor(command => command.Amount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Сумма неотрицательна.");
    }
}