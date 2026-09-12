using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchFormOptions;

namespace ZeldaArena.Web.Areas.Admin.Models.Matches;

public sealed class MatchCreateViewModel
{
    public required MatchFormViewModel Form { get; init; }

    public required MatchFormOptionsDto Options { get; init; }
}