using FluentValidation;

namespace ZeldaArena.Application.Features.Subscriptions.Commands.ExpireDueSubscriptions;

public sealed class ExpireDueSubscriptionsCommandValidator
    : AbstractValidator<ExpireDueSubscriptionsCommand>;