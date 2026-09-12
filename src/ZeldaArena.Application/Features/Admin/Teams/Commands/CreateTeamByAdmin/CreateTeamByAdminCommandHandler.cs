using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Slugs;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.CreateTeamByAdmin;

public sealed class CreateTeamByAdminCommandHandler(
    IRepository<Team> teamRepository,
    IReadRepository<Team> teams,
    IQueryExecutor queryExecutor,
    IFileStorage storage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTeamByAdminCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTeamByAdminCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = request.Name.Trim();
        var normalized = name.ToLowerInvariant();

        if (await queryExecutor.AnyAsync(teams.Query().Where(team => team.Name.ToLower() == normalized), cancellationToken))
        {
            return Result.Failure<Guid>(EsportsErrors.TeamNameTaken);
        }

        if (!await StoredImages.IsAcceptableAsync(request.Logo, cancellationToken))
        {
            return Result.Failure<Guid>(FileErrors.InvalidImage);
        }

        var slug = await SlugGenerator.UniqueAsync(
            name,
            request.Tag,
            (candidate, token) => queryExecutor.AnyAsync(teams.Query().Where(team => team.Slug == candidate), token),
            cancellationToken);

        var team = Team.Create(
            slug,
            name,
            request.Tag,
            CountryCode.From(request.CountryCode),
            request.Region,
            request.Rating,
            request.FoundedAt,
            request.Description);

        await StoredImages.ReplaceAsync(
            storage,
            request.Logo,
            remove: false,
            previous: null,
            apply: team.ChangeLogo,
            persist: async () =>
            {
                await teamRepository.AddAsync(team, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken);

        return team.Id;
    }
}