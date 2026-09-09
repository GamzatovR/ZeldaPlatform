using FluentValidation;

namespace ZeldaArena.Application.Features.Payments.Queries.GetPaymentState;

public sealed class GetPaymentStateQueryValidator : AbstractValidator<GetPaymentStateQuery>
{
    public GetPaymentStateQueryValidator() =>
        RuleFor(query => query.PaymentId)
            .NotEmpty()
            .WithMessage("Платёж не указан.");
}