using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Matches.Commands.ChangeMatchStatus;
using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchConsole;
using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchesForAdmin;
using ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;
using ZeldaArena.Web.Areas.Admin.Models.Matches;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Api.Controllers.Admin;

/// <summary>
/// Таблица матчей и пульт счёта без перезагрузки (docs/SPEC.md §10.1, сценарий 12).
///
/// Пульт отдаёт всё состояние матча, а не «счёт принят»: кнопки «Завершить» и «Отменить»
/// зависят от счёта и статуса, и решать, что теперь доступно, должен сервер. Правка
/// устаревшим пультом отвечает 409 через <c>ConcurrencyConflictException</c>
/// и <c>ApiExceptionFilter</c> — тем же кодом, что гонка за товаром в Фазе 8.
/// </summary>
[Route("api/admin/matches")]
[Authorize(Policy = PolicyNames.CanManageCatalog)]
public sealed class AdminMatchesApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminApiControllerBase(localizer)
{
    public const string TablePartial = "~/Areas/Admin/Views/Matches/_MatchTable.cshtml";

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetMatchesForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            TablePartial,
            new MatchIndexViewModel { Filter = filter, Result = result },
            AdminPageUrl(nameof(Index), "Matches"));
    }

    [HttpPost("{id:guid}/score")]
    public async Task<IActionResult> Score(Guid id, [FromForm] ScoreInputModel input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var result = await sender.Send(
            new UpdateMatchScoreCommand(id, input.ScoreA, input.ScoreB, input.ExpectedScoreA, input.ExpectedScoreB),
            cancellationToken);

        return result.IsFailure
            ? Failure(result.Error)
            : await StateAsync(id, Localizer["admin.match.score_saved"].Value, cancellationToken);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromForm] MatchTransition transition, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeMatchStatusCommand(id, transition), cancellationToken);

        return result.IsFailure
            ? Failure(result.Error)
            : await StateAsync(id, Localizer["admin.match.status_changed"].Value, cancellationToken);
    }

    private async Task<IActionResult> StateAsync(Guid id, string message, CancellationToken cancellationToken)
    {
        var match = await sender.Send(new GetMatchConsoleQuery(id), cancellationToken);

        return match is null
            ? NotFound()
            : Ok(new { message, match = MatchConsoleState.From(match) });
    }
}