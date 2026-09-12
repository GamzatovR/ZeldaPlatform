using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlanForEdit;

/// <summary>Карточка тарифа: поля, все фичи с отметками и параметрами, число подписок.</summary>
public sealed record GetPlanForEditQuery(Guid Id) : IQuery<PlanEditDto?>;