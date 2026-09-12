using FluentValidation;

using ZeldaArena.Application.Features.Admin.Matches.Commands.ScheduleMatch;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.UpdateMatch;

public sealed class UpdateMatchCommandValidator : AbstractValidator<UpdateMatchCommand>
{
    public UpdateMatchCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан матч.");

        RuleFor(command => command.StreamUrl)
            .MaximumLength(ScheduleMatchCommandValidator.MaxStreamUrlLength)
            .WithMessage($"Ссылка на трансляцию не длиннее {ScheduleMatchCommandValidator.MaxStreamUrlLength} символов.");
    }
}