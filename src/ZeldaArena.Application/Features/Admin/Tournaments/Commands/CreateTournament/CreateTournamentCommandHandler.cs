using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Slugs;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.CreateTournament;

public sealed class CreateTournamentCommandHandler(
    IRepository<Tournament> tournamentRepository,
    IReadRepository<Tournament> tournaments,
    IQueryExecutor queryExecutor,
    IHtmlSanitizer sanitizer,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTournamentCommand, Result<Guid>>
{
    private const string SlugFallback = "tournament";

    public async Task<Result<Guid>> Handle(CreateTournamentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await StoredImages.IsAcceptableAsync(request.Logo, cancellationToken))
        {
            return Result.Failure<Guid>(FileErrors.InvalidImage);
        }

        var slug = await SlugGenerator.UniqueAsync(
            request.Name,
            SlugFallback,
            (candidate, token) => queryExecutor.AnyAsync(tournaments.Query().Where(item => item.Slug == candidate), token),
            cancellationToken);

        var tournament = Tournament.Announce(
            slug,
            request.Name,
            request.Tier,
            request.Region,
            Money.FromRubles(request.PrizePool),
            request.StartsAt,
            request.EndsAt,
            request.Description,
            TournamentRulesHtml.Sanitize(sanitizer, request.RulesHtml));

        tournament.SetFeatured(request.IsFeatured);

        await StoredImages.ReplaceAsync(
            storage,
            request.Logo,
            remove: false,
            previous: null,
            apply: path => tournament.ChangeImages(path, tournament.BannerPath),
            persist: async () =>
            {
                await tournamentRepository.AddAsync(tournament, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken);

        return tournament.Id;
    }
}