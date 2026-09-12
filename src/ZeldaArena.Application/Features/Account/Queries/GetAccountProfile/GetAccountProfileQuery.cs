using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Queries.GetAccountProfile;

public sealed record GetAccountProfileQuery : IQuery<UserAccountDto?>;