using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Slugs;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Players.Commands.CreatePlayer;

public sealed class CreatePlayerCommandHandler(
    IRepository<Player> playerRepository,
    IReadRepository<Player> players,
    IQueryExecutor queryExecutor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePlayerCommand, Result<Guid>>
{
    private const string SlugFallback = "player";

    public async Task<Result<Guid>> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await StoredImages.IsAcceptableAsync(request.Avatar, cancellationToken))
        {
            return Result.Failure<Guid>(FileErrors.InvalidImage);
        }

        var slug = await SlugGenerator.UniqueAsync(
            request.Nickname,
            SlugFallback,
            (candidate, token) => queryExecutor.AnyAsync(players.Query().Where(player => player.Slug == candidate), token),
            cancellationToken);

        var player = Player.Create(
            slug,
            request.Nickname,
            CountryCode.From(request.CountryCode),
            request.Role,
            request.FirstName,
            request.LastName,
            request.BirthDate,
            request.Bio);

        await StoredImages.ReplaceAsync(
            storage,
            request.Avatar,
            remove: false,
            previous: null,
            apply: player.ChangeAvatar,
            persist: async () =>
            {
                await playerRepository.AddAsync(player, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken);

        return player.Id;
    }
}