using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Предсказуемый код подтверждения. Настоящий берётся у криптографического ГСЧ,
/// и без подмены сценарий подтверждения было бы нечем проверить.
///
/// Хеш здесь — сам код с приставкой: сверка в домене сравнивает строки, поэтому
/// от хеша требуется только детерминированность.
/// </summary>
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