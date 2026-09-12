using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchConsole;

namespace ZeldaArena.Web.Areas.Admin.Models.Matches;

public sealed class MatchConsoleViewModel
{
    public required MatchConsoleDto Match { get; init; }

    public required MatchSettingsViewModel Settings { get; init; }
}