using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Teams.Commands.UpdateMyTeamProfile;

/// <summary>
/// Слаг при переименовании не меняется: на страницу команды уже могут вести ссылки,
/// и они не должны ломаться от смены названия (слаг — адрес, а не подпись).
/// </summary>
public sealed class UpdateMyTeamProfileCommandHandler(
    IRepository<Team> teamRepository,
    IReadRepository<Team> teams,
    IQueryExecutor queryExecutor,
    IEntitlementService entitlements,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMyTeamProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateMyTeamProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var access = await MyTeamAccess
            .LoadEditableAsync(teamRepository, entitlements, currentUser, request.TeamId, cancellationToken)
            .ConfigureAwait(false);

        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var name = request.Name.Trim();
        var normalizedName = name.ToLowerInvariant();

        if (await queryExecutor
                .AnyAsync(
                    teams.Query().Where(team => team.Id != request.TeamId && team.Name.ToLower() == normalizedName),
                    cancellationToken)
                .ConfigureAwait(false))
        {
            return Result.Failure(EsportsErrors.TeamNameTaken);
        }

        access.Value.UpdateProfile(
            name,
            request.Tag,
            CountryCode.From(request.CountryCode),
            request.Region,
            request.FoundedAt,
            request.Description);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}