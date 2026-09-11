using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Features.Teams.Commands.CreateTeam;
using ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;
using ZeldaArena.Application.Features.Teams.Queries.GetTeamBySlug;
using ZeldaArena.Application.Features.Teams.Queries.GetTeams;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Teams;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Команды: список с фильтром и страница команды (docs/SPEC.md §9.3, п. 6–7).
/// Создание своей команды по подписке — здесь же, <c>/teams/create</c> (п. 8).
/// </summary>
[Route("teams")]
public sealed class TeamsController(ISender sender, IStringLocalizer<SharedResource> localizer) : Controller
{
    /// <summary>
    /// Предел тела запроса с логотипом: сам файл до 2 МБ плюс поля формы. Большее
    /// отвергается ещё до того, как дойдёт до сценария и займёт память (§15).
    /// </summary>
    private const long MaxUploadRequestBytes = ImageUploadRules.MaxSizeBytes + (256 * 1024);

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetTeamsQuery filter, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);

        return View(new TeamListViewModel { Filter = filter, Result = result });
    }

    /// <summary>
    /// Литеральный сегмент «create» сильнее параметра {slug}, поэтому маршрут не спорит
    /// со страницей команды; слаг «create» генератор к тому же не выдаёт.
    /// </summary>
    [HttpGet("create")]
    [Authorize]
    [RequireFeature(FeatureCodes.TeamCreate)]
    public IActionResult Create() => View(new CreateTeamViewModel());

    [HttpPost("create")]
    [Authorize]
    [RequireFeature(FeatureCodes.TeamCreate)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxUploadRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadRequestBytes)]
    public async Task<IActionResult> Create(CreateTeamViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var logo = model.Logo?.OpenReadStream();

        var result = await sender.Send(
            new CreateTeamCommand
            {
                Name = model.Name,
                Tag = model.Tag,
                CountryCode = model.CountryCode,
                Region = model.Region,
                FoundedAt = model.FoundedAt,
                Description = model.Description,
                Logo = model.Logo.ToFileUpload(logo),
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return View(model);
        }

        TempData["StatusMessage"] = localizer["team.created"].Value;

        return RedirectToAction(nameof(MyTeamController.Index), "MyTeam");
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var team = await sender.Send(new GetTeamBySlugQuery(slug), cancellationToken);

        if (team is null)
        {
            return NotFound();
        }

        // Отказ сценария — не ошибка страницы: у зрителя нет функции, и вместо блока
        // он увидит призыв оформить подписку.
        var advanced = await sender.Send(new GetTeamAdvancedStatsQuery(team.Id), cancellationToken);

        return View(new TeamDetailsViewModel
        {
            Team = team,
            AdvancedStats = advanced.IsSuccess ? advanced.Value : null,
        });
    }
}