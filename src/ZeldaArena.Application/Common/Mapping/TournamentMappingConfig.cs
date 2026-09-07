using Mapster;

using ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Common.Mapping;

/// <summary>
/// Правила проекции турнира в карточку списка. Объекты-значения разворачиваются
/// в примитивы явно: Money разложен на сумму и валюту, Slug — на строку.
/// Автоматического сопоставления по имени здесь не хватило бы, а угаданное молча
/// правило ломается при первом переименовании.
/// </summary>
public sealed class TournamentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.NewConfig<Tournament, TournamentListItemDto>()
            .Map(destination => destination.Slug, source => source.Slug.Value)
            .Map(destination => destination.PrizePoolAmount, source => source.PrizePool.Amount)
            .Map(destination => destination.PrizePoolCurrency, source => source.PrizePool.Currency)
            // Count() методом, а не свойством Count: свойство у IReadOnlyCollection
            // EF перевести в SQL не может и вместо COUNT(*) делает LEFT JOIN,
            // вытягивая весь состав участников каждого турнира ради одного числа
            // (docs/SPEC.md §16, обязательная проверка на N+1).
            .Map(destination => destination.ParticipantCount, source => source.Participants.Count());
    }
}