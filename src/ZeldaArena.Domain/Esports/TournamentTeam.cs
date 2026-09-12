using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Domain.Esports;

/// <summary>Участие команды в турнире.</summary>
public class TournamentTeam
{
    private TournamentTeam()
    {
    }

    public Guid TournamentId { get; private set; }

    public Guid TeamId { get; private set; }

    /// <summary>Посев: номер команды в сетке.</summary>
    public int Seed { get; private set; }

    /// <summary>Итоговое место. Пусто, пока турнир не завершён.</summary>
    public int? Placement { get; private set; }

    public Tournament? Tournament { get; private set; }

    public Team? Team { get; private set; }

    internal static TournamentTeam Create(Guid tournamentId, Guid teamId, int seed)
    {
        InvariantViolationException.ThrowIf(
            seed < 1,
            "tournament_team.invalid_seed",
            $"Посев начинается с единицы, получено {seed}.");

        return new TournamentTeam
        {
            TournamentId = tournamentId,
            TeamId = teamId,
            Seed = seed,
        };
    }

    internal void SetPlacement(int placement)
    {
        InvariantViolationException.ThrowIf(
            placement < 1,
            "tournament_team.invalid_placement",
            $"Место начинается с первого, получено {placement}.");

        Placement = placement;
    }

    internal void ChangeSeed(int seed)
    {
        InvariantViolationException.ThrowIf(
            seed < 1,
            "tournament_team.invalid_seed",
            $"Посев начинается с единицы, получено {seed}.");

        Seed = seed;
    }
}