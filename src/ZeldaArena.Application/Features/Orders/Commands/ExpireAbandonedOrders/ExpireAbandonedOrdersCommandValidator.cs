using FluentValidation;

namespace ZeldaArena.Application.Features.Orders.Commands.ExpireAbandonedOrders;

/// <summary>Параметров нет; валидатор — ради соглашения «команда + хендлер + валидатор».</summary>
public sealed class ExpireAbandonedOrdersCommandValidator : AbstractValidator<ExpireAbandonedOrdersCommand>;