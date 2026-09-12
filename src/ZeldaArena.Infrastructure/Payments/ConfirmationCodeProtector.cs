using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Infrastructure.Payments;

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