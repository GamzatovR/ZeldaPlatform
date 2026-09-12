using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Application.Common.Rules;

public static class DomainRules
{
    public static Result Apply(Action operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        try
        {
            operation();

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(new Error(exception.Code, exception.Message));
        }
    }
}