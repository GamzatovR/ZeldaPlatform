using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.UpdateTournament;

public sealed class UpdateTournamentCommandHandler(
    IRepository<Tournament> tournamentRepository,
    IHtmlSanitizer sanitizer,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTournamentCommand, Result>
{
    public async Task<Result> Handle(UpdateTournamentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tournament = await tournamentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (tournament is null)
        {
            return Result.Failure(EsportsErrors.TournamentNotFound);
        }

        if (!await StoredImages.IsAcceptableAsync(request.Logo, cancellationToken))
        {
            return Result.Failure(FileErrors.InvalidImage);
        }

        var updated = DomainRules.Apply(() => tournament.UpdateDetails(
            request.Name,
            request.Tier,
            request.Region,
            Money.FromRubles(request.PrizePool),
            request.StartsAt,
            request.EndsAt,
            request.Description,
            TournamentRulesHtml.Sanitize(sanitizer, request.RulesHtml)));

        if (updated.IsFailure)
        {
            return updated;
        }

        tournament.SetFeatured(request.IsFeatured);

        await StoredImages.ReplaceAsync(
            storage,
            request.Logo,
            request.RemoveLogo,
            tournament.LogoPath,
            path => tournament.ChangeImages(path, tournament.BannerPath),
            () => unitOfWork.SaveChangesAsync(cancellationToken),
            cancellationToken);

        return Result.Success();
    }
}