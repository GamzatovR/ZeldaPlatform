using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Queries.IsEmailAvailable;

/// <summary>
/// Свободен ли адрес для регистрации — Remote-валидация формы регистрации
/// (docs/SPEC.md §10.1, сценарий 11; §15).
///
/// Ответ говорит, заведена ли учётная запись на адрес, но нового об этом не сообщает:
/// сама регистрация отвечает «адрес занят» с Фазы 3. Перебор адресов этим запросом
/// сдерживает ограничение частоты на эндпоинте.
/// </summary>
public sealed record IsEmailAvailableQuery(string Email) : IQuery<bool>;
