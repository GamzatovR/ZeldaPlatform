using ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamForEdit;

namespace ZeldaArena.Web.Areas.Admin.Models.Teams;

public sealed class TeamEditViewModel
{
    public required TeamEditDto Team { get; init; }

    public required TeamFormViewModel Form { get; init; }
}