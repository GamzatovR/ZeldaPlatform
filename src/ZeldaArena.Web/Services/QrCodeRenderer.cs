using QRCoder;

namespace ZeldaArena.Web.Services;

/// <summary>
/// Рисует QR-код ссылки otpauth:// как встроенный SVG (docs/SPEC.md §8.2).
///
/// На сервере, а не на клиенте: так код виден и с выключенным JavaScript,
/// и в проект не приходится тащить вендорскую библиотеку в wwwroot — требование
/// по минификации и объёму статики из §16 остаётся выполнимым.
///
/// SVG, а не PNG в data-URI: он вчетверо легче и остаётся чётким при любом масштабе.
/// </summary>
public static class QrCodeRenderer
{
    /// <summary>Размер модуля в пикселях: при 4 код читается камерой телефона с экрана.</summary>
    private const int PixelsPerModule = 4;

    public static string ToSvg(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        using var generator = new QRCodeGenerator();

        // Q — коррекция ошибок 25%: запас на блики и муар при съёмке с монитора.
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

        return new SvgQRCode(data).GetGraphic(PixelsPerModule);
    }
}