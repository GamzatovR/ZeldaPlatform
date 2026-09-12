using FluentValidation;

namespace ZeldaArena.Application.Features.Files.Queries.GetStoredFile;

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