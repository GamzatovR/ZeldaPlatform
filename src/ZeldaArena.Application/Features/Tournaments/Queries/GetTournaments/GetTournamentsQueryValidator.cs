using FluentValidation;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;

/// <summary>
/// Проверяются только настоящие ошибки: отрицательная страница, слишком длинный поиск,
/// значение вне перечисления.
///
/// Размер страницы и ключ сортировки сюда намеренно не попали — они нормализуются молча.
/// Ссылку на список сохраняют в закладки и пересылают, и она обязана открыться даже после
/// того, как сортировку переименовали, а не встречать пользователя ошибкой (§10.2).
/// </summary>
public sealed class GetTournamentsQueryValidator : AbstractValidator<GetTournamentsQuery>
{
    public const int MaxSearchLength = 100;

    public GetTournamentsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Search)
            .MaximumLength(MaxSearchLength)
            .WithMessage($"Строка поиска не длиннее {MaxSearchLength} символов.");

        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue)
            .WithMessage("Неизвестный статус турнира.");

        RuleFor(query => query.PrizeMin)
            .GreaterThanOrEqualTo(0)
            .When(query => query.PrizeMin.HasValue)
            .WithMessage("Призовой фонд не может быть отрицательным.");

        RuleFor(query => query.To)
            .GreaterThanOrEqualTo(query => query.From)
            .When(query => query.From.HasValue && query.To.HasValue)
            .WithMessage("Дата «по» не может быть раньше даты «с».");

        RuleFor(query => query.Region)
            .IsInEnum()
            .When(query => query.Region.HasValue)
            .WithMessage("Неизвестный регион.");
    }
}