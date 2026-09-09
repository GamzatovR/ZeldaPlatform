using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlansForAdmin;

/// <summary>Все тарифы — страница /admin/plans (docs/SPEC.md §9.4).</summary>
public sealed record GetPlansForAdminQuery : IQuery<IReadOnlyList<AdminPlanDto>>;