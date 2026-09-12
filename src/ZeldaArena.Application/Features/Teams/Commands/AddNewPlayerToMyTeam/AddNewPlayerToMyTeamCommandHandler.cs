using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Slugs;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Teams.Commands.AddNewPlayerToMyTeam;

public sealed class AddNewPlayerToMyTeamCommandHandler(
    IRepository<Team> teamRepository,
    IRepository<Player> playerRepository,
    IReadRepository<Player> players,
    IQueryExecutor queryExecutor,
    IEntitlementService entitlements,
    ICurrentUserService currentUser,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddNewPlayerToMyTeamCommand, Result>
{
    /// <summary>Запасная основа слага, если из ника ничего не складывается.</summary>
    private const string FallbackSlug = "player";

    public async Task<Result> Handle(AddNewPlayerToMyTeamCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var access = await MyTeamAccess
            .LoadEditableAsync(teamRepository, entitlements, currentUser, request.TeamId, cancellationToken)
            .ConfigureAwait(false);

        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var slug = await SlugGenerator.UniqueAsync(
            request.Nickname,
            FallbackSlug,
            (candidate, token) => queryExecutor.AnyAsync(players.Query().Where(player => player.Slug == candidate), token),
            cancellationToken).ConfigureAwait(false);

        var player = Player.Create(
            slug,
            request.Nickname.Trim(),
            CountryCode.From(request.CountryCode),
            request.Role,
            request.FirstName,
            request.LastName);

        await playerRepository.AddAsync(player, cancellationToken).ConfigureAwait(false);
        access.Value.AddPlayer(player.Id, request.Role, clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}