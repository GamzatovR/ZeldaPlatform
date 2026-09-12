using FluentValidation;

namespace ZeldaArena.Application.Features.Account.Queries.GetAccountProfile;

/// <summary>
/// Проверять нечего: у запроса нет параметров, а личность берётся из текущего запроса.
/// Валидатор заведён, чтобы срез не выбивался из соглашения «команда/запрос —
/// хендлер — валидатор» (docs/CONVENTIONS.md) и чтобы правила было куда дописать.
/// </summary>
public sealed class GetAccountProfileQueryValidator : AbstractValidator<GetAccountProfileQuery>;