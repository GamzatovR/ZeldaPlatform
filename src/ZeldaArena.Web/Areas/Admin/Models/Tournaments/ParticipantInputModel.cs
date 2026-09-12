using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Admin.Tournaments.Commands.AddTournamentTeam;

namespace ZeldaArena.Web.Areas.Admin.Models.Tournaments;

public sealed class ParticipantInputModel
{
    public Guid TeamId { get; set; }

    [Range(1, AddTournamentTeamCommandValidator.MaxSeed, ErrorMessage = "Посев — от {1} до {2}.")]
    public int Seed { get; set; } = 1;

    [Range(1, AddTournamentTeamCommandValidator.MaxSeed, ErrorMessage = "Место — от {1} до {2}.")]
    public int? Placement { get; set; }
}