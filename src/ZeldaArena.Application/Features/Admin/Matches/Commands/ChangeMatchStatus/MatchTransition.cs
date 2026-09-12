namespace ZeldaArena.Application.Features.Admin.Matches.Commands.ChangeMatchStatus;

/// <summary>
/// Переход состояния матча из пульта. Как и у турнира, передаётся действие, а не
/// целевой статус: допустимость перехода знает сущность.
/// </summary>
public enum MatchTransition
{
    Start = 0,
    Finish = 1,
    Cancel = 2,
}