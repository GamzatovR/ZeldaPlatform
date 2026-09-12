using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Models.Esports;

public static class EsportsErrors
{
    public static readonly Error TeamNotFound =
        new("team.not_found", "Команда не найдена.");

    public static readonly Error PlayerNotFound =
        new("player.not_found", "Игрок не найден.");

    public static readonly Error FeatureRequired =
        new("feature.required", "Эта функция доступна по подписке.");

    /// <summary>Лимит команд на владельца — параметр фичи team.create.</summary>
    public static readonly Error TeamLimitReached =
        new("team.limit_reached", "Достигнут лимит команд для вашей подписки.");

    public static readonly Error TeamNameTaken =
        new("team.name_taken", "Команда с таким названием уже есть.");

    /// <summary>Игрок не может состоять в двух командах одновременно.</summary>
    public static readonly Error PlayerInAnotherTeam =
        new("player.already_in_team", "Игрок уже состоит в другой команде.");

    public static readonly Error TournamentNotFound =
        new("tournament.not_found", "Турнир не найден.");

    public static readonly Error MatchNotFound =
        new("match.not_found", "Матч не найден.");

    public static readonly Error MatchHasHistory =
        new("match.has_history", "Матч уже начинался — удалить его нельзя, можно отменить.");

    public static readonly Error TeamNotInTournament =
        new("match.team_not_in_tournament", "Обе команды должны участвовать в турнире.");

    public static readonly Error TournamentHasMatches =
        new("tournament.has_matches", "У турнира есть матчи — удалить его нельзя, можно отменить.");

    public static readonly Error ParticipantHasMatches =
        new("tournament.participant_has_matches", "У команды есть матчи в этом турнире — убрать её нельзя.");

    public static readonly Error PlayerHasHistory =
        new("player.has_history", "Игрок состоял в командах или имеет статистику — удалить его нельзя.");

    public static readonly Error TeamHasHistory =
        new("team.has_history", "У команды есть матчи, турниры или история состава — удалить её нельзя.");

    public static readonly Error TeamNotApproved =
        new("tournament.team_not_approved", "Команда ещё не одобрена и не может участвовать в турнирах.");
}