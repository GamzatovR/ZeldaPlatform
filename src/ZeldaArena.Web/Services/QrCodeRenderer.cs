using QRCoder;

namespace ZeldaArena.Web.Services;

/// <summary>
/// Рисует QR-код ссылки otpauth:// как SVG в data-URI для <c>&lt;img&gt;</c> (docs/SPEC.md §8.2).
///
/// На сервере, а не на клиенте: так код виден и с выключенным JavaScript,
/// и в проект не приходится тащить вендорскую библиотеку в wwwroot — требование
/// по минификации и объёму статики из §16 остаётся выполнимым.
///
/// SVG, а не PNG: он вчетверо легче и остаётся чётким при любом масштабе.
///
/// В <c>&lt;img&gt;</c>, а не встроенной разметкой через <c>Html.Raw</c>: сырой вывод
/// разрешён только двум санитизированным полям (§15), и архитектурный тест следит,
/// чтобы исключений не появлялось. Картинка к тому же не исполняет скриптов внутри SVG,
/// что бы в него ни попало.
/// </summary>
public static class QrCodeRenderer
{
    /// <summary>Размер модуля в пикселях: при 4 код читается камерой телефона с экрана.</summary>
    private const int PixelsPerModule = 4;

    public static string ToSvgDataUri(string payload) =>
        "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(ToSvg(payload)));

    private static string ToSvg(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        using var generator = new QRCodeGenerator();

        // Q — коррекция ошибок 25%: запас на блики и муар при съёмке с монитора.
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

        return new SvgQRCode(data).GetGraphic(PixelsPerModule);
    }
}