using ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;

namespace ZeldaArena.Web.Areas.Admin.Models.Tournaments;

public sealed class TournamentEditViewModel
{
    public required TournamentEditDto Tournament { get; init; }

    public required TournamentFormViewModel Form { get; init; }
}