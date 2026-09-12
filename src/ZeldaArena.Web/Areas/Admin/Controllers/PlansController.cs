using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Billing.Commands.CreatePlan;
using ZeldaArena.Application.Features.Admin.Billing.Commands.DeletePlan;
using ZeldaArena.Application.Features.Admin.Billing.Commands.SetPlanFeatures;
using ZeldaArena.Application.Features.Admin.Billing.Commands.UpdatePlan;
using ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlanForEdit;
using ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlansForAdmin;
using ZeldaArena.Web.Areas.Admin.Models.Billing;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

[Route("admin/plans")]
[Authorize(Policy = PolicyNames.CanManageBilling)]
public sealed class PlansController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await sender.Send(new GetPlansForAdminQuery(), cancellationToken));

    [HttpGet("create")]
    public IActionResult Create() => View(new PlanFormViewModel());

    [HttpPost("create")]
    public async Task<IActionResult> Create(PlanFormViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await sender.Send(
            new CreatePlanCommand(
                model.Code,
                model.Name,
                model.Description,
                model.Price,
                model.DurationDays,
                model.SortOrder),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return View(model);
        }

        ReportSuccess(localizer["admin.plan.created"].Value);

        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var plan = await sender.Send(new GetPlanForEditQuery(id), cancellationToken);

        return plan is null
            ? NotFound()
            : View(new PlanEditViewModel { Plan = plan, Form = PlanFormViewModel.From(plan) });
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Edit(
        Guid id,
        [Bind(Prefix = nameof(PlanEditViewModel.Form))] PlanFormViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (ModelState.IsValid)
        {
            var result = await sender.Send(
                new UpdatePlanCommand(
                    id,
                    model.Name,
                    model.Description,
                    model.Price,
                    model.DurationDays,
                    model.SortOrder,
                    model.IsActive),
                cancellationToken);

            if (result.IsSuccess)
            {
                ReportSuccess(localizer["admin.saved"].Value);

                return RedirectToAction(nameof(Edit), new { id });
            }

            ModelState.AddResultError(result, localizer);
        }

        var plan = await sender.Send(new GetPlanForEditQuery(id), cancellationToken);

        return plan is null
            ? NotFound()
            : View(new PlanEditViewModel { Plan = plan, Form = model });
    }

    [HttpPost("{id:guid}/features")]
    public async Task<IActionResult> SetFeatures(
        Guid id,
        [FromForm] IReadOnlyList<PlanFeatureInputModel> features,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(features);

        var assignments = features
            .Where(feature => feature.IsGranted)
            .Select(feature => new PlanFeatureAssignment(
                feature.FeatureId,
                string.IsNullOrWhiteSpace(feature.Value) ? null : feature.Value.Trim()))
            .ToList();

        var result = await sender.Send(new SetPlanFeaturesCommand(id, assignments), cancellationToken);
        Report(result, localizer["admin.plan.features_saved"].Value, localizer);

        return RedirectToAction(nameof(Edit), null, new { id }, "features");
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeletePlanCommand(id), cancellationToken);
        Report(result, localizer["admin.plan.deleted"].Value, localizer);

        return result.IsSuccess
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Edit), new { id });
    }
}