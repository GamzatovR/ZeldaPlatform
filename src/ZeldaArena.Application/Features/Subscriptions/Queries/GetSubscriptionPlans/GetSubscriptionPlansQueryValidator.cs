using FluentValidation;

namespace ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;

/// <summary>Параметров у запроса нет, но валидатор заводится по соглашению о срезах.</summary>
public sealed class GetSubscriptionPlansQueryValidator : AbstractValidator<GetSubscriptionPlansQuery>;