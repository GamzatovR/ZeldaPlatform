using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Commands.UpdateMatch;

/// <summary>
/// Время меняется двумя разными методами сущности: у запланированного матча это
/// перенос времени (<c>Reschedule</c>), у уже отложенного — повторная отсрочка
/// (<c>Postpone</c>), которая сохраняет статус «перенесён». Идущий и завершённый
/// матч времени не меняет — так решает сущность.
/// </summary>
public sealed class UpdateMatchCommandHandler(
    IRepository<Match> matches,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMatchCommand, Result>
{
    public async Task<Result> Handle(UpdateMatchCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var match = await matches.GetByIdAsync(request.Id, cancellationToken);

        if (match is null)
        {
            return Result.Failure(EsportsErrors.MatchNotFound);
        }

        var result = DomainRules.Apply(() =>
        {
            if (match.ScheduledAt != request.ScheduledAt)
            {
                if (match.Status == MatchStatus.Postponed)
                {
                    match.Postpone(request.ScheduledAt);
                }
                else
                {
                    match.Reschedule(request.ScheduledAt);
                }
            }

            match.SetStreamUrl(request.StreamUrl);
        });

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}