using FluentValidation;

namespace ZeldaArena.Application.Features.Files.Queries.GetStoredFile;

/// <summary>
/// Здесь только грубые границы. Вид имени проверяет хранилище: чужое имя — это
/// «такого файла нет» и ответ 404, а не ошибка валидации и 500 на ссылке,
/// которую кто-то перепечатал с опечаткой. Выход за каталог хранилища закрыт там же,
/// где файл открывается, — ближе к диску проверка не бывает.
/// </summary>
public sealed class GetStoredFileQueryValidator : AbstractValidator<GetStoredFileQuery>
{
    public const int MaxNameLength = 64;

    public GetStoredFileQueryValidator()
    {
        RuleFor(query => query.Name)
            .NotEmpty()
            .MaximumLength(MaxNameLength);
    }
}