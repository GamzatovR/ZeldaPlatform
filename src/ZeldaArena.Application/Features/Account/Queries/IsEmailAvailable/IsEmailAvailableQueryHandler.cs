using MediatR;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.Account.Queries.IsEmailAvailable;

/// <summary>
/// Адрес свободен, если на него не заведена ни одна учётная запись. Сравнение
/// регистронезависимое — его делает Identity по нормализованному адресу, тем же
/// способом, каким регистрация потом отвергла бы занятый адрес.
/// </summary>
public sealed class IsEmailAvailableQueryHandler(IUserAccountService userAccounts)
    : IRequestHandler<IsEmailAvailableQuery, bool>
{
    public async Task<bool> Handle(IsEmailAvailableQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await userAccounts
            .FindByEmailAsync(request.Email.Trim(), cancellationToken)
            .ConfigureAwait(false);

        return existing is null;
    }
}
