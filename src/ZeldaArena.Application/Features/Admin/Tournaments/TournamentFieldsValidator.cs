using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Admin.Tournaments;

/// <summary>
/// Правила полей турнира. Длины совпадают со столбцами <c>TournamentConfiguration</c>,
/// порядок дат — с инвариантом <c>Tournament</c>: форма не должна принимать то,
/// что потом отвергнет база или домен.
/// </summary>
public sealed class TournamentFieldsValidator : AbstractValidator<ITournamentFields>
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 4000;

    /// <summary>Регламент — это страница текста, а не книга; предел защищает базу от мусора.</summary>
    public const int MaxRulesLength = 20_000;

    public const decimal MaxPrizePool = 1_000_000_000m;

    public TournamentFieldsValidator()
    {
        RuleFor(fields => fields.Name)
            .NotEmpty()
            .WithMessage("Укажите название турнира.")
            .MaximumLength(MaxNameLength)
            .WithMessage($"Название не длиннее {MaxNameLength} символов.");

        RuleFor(fields => fields.Tier)
            .IsInEnum()
            .WithMessage("Неизвестный уровень турнира.");

        RuleFor(fields => fields.Region)
            .IsInEnum()
            .WithMessage("Неизвестный регион.");

        RuleFor(fields => fields.PrizePool)
            .InclusiveBetween(0m, MaxPrizePool)
            .WithMessage("Призовой фонд — от нуля до миллиарда.");

        RuleFor(fields => fields.EndsAt)
            .GreaterThanOrEqualTo(fields => fields.StartsAt)
            .WithMessage("Турнир не может закончиться раньше, чем начнётся.");

        RuleFor(fields => fields.Description)
            .MaximumLength(MaxDescriptionLength)
            .WithMessage($"Описание не длиннее {MaxDescriptionLength} символов.");

        RuleFor(fields => fields.RulesHtml)
            .MaximumLength(MaxRulesLength)
            .WithMessage($"Регламент не длиннее {MaxRulesLength} символов.");

        RuleFor(fields => fields.Logo).ValidImageUpload();
    }
}