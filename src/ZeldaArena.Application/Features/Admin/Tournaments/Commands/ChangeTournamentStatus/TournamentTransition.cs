namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.ChangeTournamentStatus;

/// <summary>
/// Переход, а не целевой статус: статус не выставляется произвольно, а меняется
/// методами <c>Tournament</c>, и допустимость перехода решает сущность.
/// </summary>
public enum TournamentTransition
{
    Start = 0,
    Finish = 1,
    Cancel = 2,
}