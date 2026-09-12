using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Models.Esports;

/// <summary>
/// Ошибки киберспортивных сценариев. Коды — ключи ресурсов (docs/SPEC.md §9.5),
/// собраны в одном месте по образцу <c>BillingErrors</c>.
/// </summary>
public static class EsportsErrors
{
    public static readonly Error TeamNotFound =
        new("team.not_found", "Команда не найдена.");

    public static readonly Error PlayerNotFound =
        new("player.not_found", "Игрок не найден.");

    /// <summary>
    /// Платной функции нет. Сценарий проверяет её сам, а не полагается на атрибут
    /// действия: тот же сценарий вызовут Areas/Api и мобильный клиент (EP-2),
    /// и защита, живущая только в контроллере, до них не доедет (docs/SPEC.md §7.3).
    /// </summary>
    public static readonly Error FeatureRequired =
        new("feature.required", "Эта функция доступна по подписке.");

    /// <summary>Лимит команд на владельца — параметр фичи team.create (EP-5, §15).</summary>
    public static readonly Error TeamLimitReached =
        new("team.limit_reached", "Достигнут лимит команд для вашей подписки.");

    public static readonly Error TeamNameTaken =
        new("team.name_taken", "Команда с таким названием уже есть.");

    /// <summary>Игрок не может состоять в двух командах одновременно (§15).</summary>
    public static readonly Error PlayerInAnotherTeam =
        new("player.already_in_team", "Игрок уже состоит в другой команде.");

    public static readonly Error TournamentNotFound =
        new("tournament.not_found", "Турнир не найден.");

    public static readonly Error TournamentHasMatches =
        new("tournament.has_matches", "У турнира есть матчи — удалить его нельзя, можно отменить.");

    public static readonly Error ParticipantHasMatches =
        new("tournament.participant_has_matches", "У команды есть матчи в этом турнире — убрать её нельзя.");

    public static readonly Error TeamNotApproved =
        new("tournament.team_not_approved", "Команда ещё не одобрена и не может участвовать в турнирах.");
}