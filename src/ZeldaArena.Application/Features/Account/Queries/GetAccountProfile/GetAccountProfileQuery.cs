using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Queries.GetAccountProfile;

/// <summary>
/// Профиль текущего пользователя для личного кабинета.
///
/// Параметров нет намеренно: чей профиль показывать, определяет запрос, а не форма.
/// Идентификатор в query-string открыл бы чтение чужих профилей (docs/SPEC.md §15, IDOR).
/// </summary>
public sealed record GetAccountProfileQuery : IQuery<UserAccountDto?>;