using FluentValidation;

namespace ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;

/// <summary>
/// Проверяет форму запроса: идентификатор задан, счёт неотрицателен и не выходит
/// за разумные пределы.
///
/// Соответствие счёта формату серии здесь не проверяется намеренно: для этого нужно
/// знать BestOf конкретного матча, а инвариант матча — забота самой сущности
/// (Match.UpdateScore). Дублирование правила в валидаторе означало бы два места,
/// где его придётся менять, и одно из них рано или поздно забудут.
/// </summary>
public sealed class UpdateMatchScoreCommandValidator : AbstractValidator<UpdateMatchScoreCommand>
{
    /// <summary>Формат серии в проекте не длиннее Bo7, запас взят с большим избытком.</summary>
    public const int MaxScore = 100;

    public UpdateMatchScoreCommandValidator()
    {
        RuleFor(command => command.MatchId)
            .NotEmpty()
            .WithMessage("Не указан матч.");

        RuleFor(command => command.ScoreA)
            .InclusiveBetween(0, MaxScore)
            .WithMessage($"Счёт первой команды должен быть от 0 до {MaxScore}.");

        RuleFor(command => command.ScoreB)
            .InclusiveBetween(0, MaxScore)
            .WithMessage($"Счёт второй команды должен быть от 0 до {MaxScore}.");
    }
}