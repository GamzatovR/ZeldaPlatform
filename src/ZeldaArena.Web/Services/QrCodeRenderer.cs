using QRCoder;

namespace ZeldaArena.Web.Services;

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