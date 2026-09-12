using ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlanForEdit;

namespace ZeldaArena.Web.Areas.Admin.Models.Billing;

public sealed class PlanEditViewModel
{
    public required PlanEditDto Plan { get; init; }

    public required PlanFormViewModel Form { get; init; }
}