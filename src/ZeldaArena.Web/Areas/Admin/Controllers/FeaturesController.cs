using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Billing.Commands.CreateFeature;
using ZeldaArena.Application.Features.Admin.Billing.Commands.DeleteFeature;
using ZeldaArena.Application.Features.Admin.Billing.Commands.ToggleFeature;
using ZeldaArena.Application.Features.Admin.Billing.Commands.UpdateFeature;
using ZeldaArena.Application.Features.Admin.Billing.Queries.GetFeatures;
using ZeldaArena.Web.Areas.Admin.Models.Billing;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

[Route("admin/features")]
[Authorize(Policy = PolicyNames.CanManageBilling)]
public sealed class FeaturesController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await sender.Send(new GetFeaturesQuery(), cancellationToken));

    [HttpPost("create")]
    public async Task<IActionResult> Create(FeatureFormViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            ReportFailure(FirstModelError());
        }
        else
        {
            var result = await sender.Send(
                new CreateFeatureCommand(model.Code, model.Name, model.Description),
                cancellationToken);

            Report(result, localizer["admin.feature.created"].Value, localizer);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, FeatureFormViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        // Код фичи не меняется, поэтому его ошибки на этой форме не смотрим.
        ModelState.Remove(nameof(FeatureFormViewModel.Code));

        if (!ModelState.IsValid)
        {
            ReportFailure(FirstModelError());
        }
        else
        {
            var result = await sender.Send(new UpdateFeatureCommand(id, model.Name, model.Description), cancellationToken);
            Report(result, localizer["admin.saved"].Value, localizer);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ToggleFeatureCommand(id, isActive), cancellationToken);
        Report(result, localizer[isActive ? "admin.feature.enabled" : "admin.feature.disabled"].Value, localizer);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteFeatureCommand(id), cancellationToken);
        Report(result, localizer["admin.feature.deleted"].Value, localizer);

        return RedirectToAction(nameof(Index));
    }
}