using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Infrastructure.Payments;

/// <summary>
/// Выпуск и хеширование кода подтверждения оплаты (docs/SPEC.md §7.6).
///
/// Код берётся у криптографического ГСЧ, а не у <c>Random</c>: предсказуемый код
/// позволял бы подтвердить чужой платёж, не заглядывая в почту.
///
/// Хеш — SHA-256 от перца и кода. Соли на запись нет намеренно: <c>Payment.Confirm</c>
/// сверяет хеши строковым сравнением постоянного времени, и для этого хеш обязан
/// зависеть только от кода. Роль секрета играет перец, общий для приложения
/// и не попадающий в дамп базы. Медленная функция здесь не нужна: код живёт десять
/// минут и переживает пять попыток, а перебор миллиона вариантов ловится не стоимостью
/// хеша, а этим счётчиком.
/// </summary>
public sealed class ConfirmationCodeProtector(IOptions<ConfirmationCodeOptions> options)
    : IConfirmationCodeProtector
{
    private static readonly int UpperBound = (int)Math.Pow(10, PaymentPolicy.CodeLength);

    private readonly string _pepper = options.Value.Pepper;

    public ConfirmationCode Issue()
    {
        var code = RandomNumberGenerator
            .GetInt32(UpperBound)
            .ToString(CultureInfo.InvariantCulture)
            .PadLeft(PaymentPolicy.CodeLength, '0');

        return new ConfirmationCode(code, Hash(code));
    }

    public string Hash(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var bytes = Encoding.UTF8.GetBytes(_pepper + code.Trim());

        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }
}