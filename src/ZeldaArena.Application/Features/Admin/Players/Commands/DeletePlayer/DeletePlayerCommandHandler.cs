using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Players.Commands.DeletePlayer;

public sealed class DeletePlayerCommandHandler(
    IRepository<Player> players,
    IReadRepository<RosterEntry> rosterEntries,
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePlayerCommand, Result>
{
    public async Task<Result> Handle(DeletePlayerCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var player = await players.GetByIdAsync(request.Id, cancellationToken);

        if (player is null)
        {
            return Result.Failure(EsportsErrors.PlayerNotFound);
        }

        var id = request.Id;

        var hasHistory = await queryExecutor.AnyAsync(
                rosterEntries.Query().Where(entry => entry.PlayerId == id),
                cancellationToken)
            || await queryExecutor.AnyAsync(
                matches.Query().SelectMany(match => match.PlayerStats).Where(stats => stats.PlayerId == id),
                cancellationToken);

        if (hasHistory)
        {
            return Result.Failure(EsportsErrors.PlayerHasHistory);
        }

        var avatar = player.AvatarPath;

        players.Remove(player);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (avatar is not null)
        {
            await storage.DeleteAsync(avatar, cancellationToken);
        }

        return Result.Success();
    }
}