using Mapster;

using ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Common.Mapping;

public sealed class TournamentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.NewConfig<Tournament, TournamentListItemDto>()
            .Map(destination => destination.Slug, source => source.Slug.Value)
            .Map(destination => destination.PrizePoolAmount, source => source.PrizePool.Amount)
            .Map(destination => destination.PrizePoolCurrency, source => source.PrizePool.Currency)
            // Count методом, а не свойством Count.
            .Map(destination => destination.ParticipantCount, source => source.Participants.Count());
    }
}