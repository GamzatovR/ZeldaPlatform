using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.ScheduleMatch;

public sealed class ScheduleMatchCommandValidator : AbstractValidator<ScheduleMatchCommand>
{
    /// <summary>Форматов длиннее Bo7 в проекте нет; нечётность проверяет сама сущность.</summary>
    public const int MaxBestOf = 7;

    public const int MaxStreamUrlLength = 400;

    public ScheduleMatchCommandValidator()
    {
        RuleFor(command => command.TournamentId).NotEmpty().WithMessage("Не указан турнир.");
        RuleFor(command => command.TeamAId).NotEmpty().WithMessage("Выберите первую команду.");
        RuleFor(command => command.TeamBId).NotEmpty().WithMessage("Выберите вторую команду.");

        RuleFor(command => command.TeamBId)
            .NotEqual(command => command.TeamAId)
            .WithMessage("Команда не может играть сама с собой.");

        RuleFor(command => command.BestOf)
            .InclusiveBetween(1, MaxBestOf)
            .WithMessage($"Формат серии — от Bo1 до Bo{MaxBestOf}.");

        RuleFor(command => command.StreamUrl)
            .MaximumLength(MaxStreamUrlLength)
            .WithMessage($"Ссылка на трансляцию не длиннее {MaxStreamUrlLength} символов.");
    }
}