using ZeldaArena.Application.Common.Models.Billing;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IConfirmationCodeProtector
{
    /// <summary>Новый код и его хеш. Код отдаётся вызывающему только чтобы отправить письмо.</summary>
    ConfirmationCode Issue();

    string Hash(string code);
}