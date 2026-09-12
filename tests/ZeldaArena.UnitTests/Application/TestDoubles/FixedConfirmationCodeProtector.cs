using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class FixedConfirmationCodeProtector(string code = "123456")
    : IConfirmationCodeProtector
{
    public string Code { get; private set; } = code;

    public int Issued { get; private set; }

    public ConfirmationCode Issue()
    {
        Issued++;

        return new ConfirmationCode(Code, Hash(Code));
    }

    /// <summary>Следующая выдача вернёт другой код — так проверяется, что прежний перестал работать.</summary>
    public void NextCodeWillBe(string code) => Code = code;

    public string Hash(string code) => "hash:" + code.Trim();
}