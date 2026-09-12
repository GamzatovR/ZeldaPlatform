using ZeldaArena.Application.Common.Files;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Common.Files;

public class ImageFileInspectorTests
{
    [Fact]
    public void Png_is_recognised_by_its_signature() =>
        ImageFileInspector.Detect(SampleImages.Png).ShouldBe(ImageFormat.Png);

    [Fact]
    public void Jpeg_is_recognised_by_its_signature() =>
        ImageFileInspector.Detect(SampleImages.Jpeg).ShouldBe(ImageFormat.Jpeg);

    [Fact]
    public void Webp_is_recognised_by_the_riff_container_and_its_type() =>
        ImageFileInspector.Detect(SampleImages.Webp).ShouldBe(ImageFormat.Webp);

    [Fact]
    public void Executable_renamed_to_png_is_not_an_image() =>
        ImageFileInspector.Detect(SampleImages.Executable).ShouldBeNull();

    /// <summary>RIFF — это ещё и WAV, и AVI: одного начала контейнера мало.</summary>
    [Fact]
    public void Riff_container_of_another_type_is_not_webp() =>
        ImageFileInspector.Detect([.. "RIFF"u8, 0x24, 0, 0, 0, .. "WAVE"u8]).ShouldBeNull();

    [Fact]
    public void Truncated_header_is_not_an_image() =>
        ImageFileInspector.Detect(SampleImages.Png.AsSpan(0, 4)).ShouldBeNull();

    [Fact]
    public void Empty_content_is_not_an_image() =>
        ImageFileInspector.Detect([]).ShouldBeNull();

    [Fact]
    public async Task Detection_returns_the_stream_to_its_start()
    {
        using var stream = SampleImages.Png.AsStream();

        var format = await ImageFileInspector.DetectAsync(stream, CancellationToken.None);

        format.ShouldBe(ImageFormat.Png);
        stream.Position.ShouldBe(0);
    }

    [Theory]
    [InlineData("logo.png", true)]
    [InlineData("LOGO.JPG", true)]
    [InlineData("photo.jpeg", true)]
    [InlineData("banner.webp", true)]
    [InlineData("logo.gif", false)]
    [InlineData("logo.png.exe", false)]
    [InlineData("logo", false)]
    [InlineData("", false)]
    public void Only_whitelisted_extensions_are_allowed(string fileName, bool expected) =>
        ImageUploadRules.HasAllowedExtension(fileName).ShouldBe(expected);

    [Theory]
    [InlineData("0123456789abcdef0123456789abcdef.png", true)]
    [InlineData("0123456789abcdef0123456789abcdef.webp", true)]
    [InlineData("../0123456789abcdef0123456789abcdef.png", false)]
    [InlineData("0123456789abcdef0123456789abcdef.png/..", false)]
    [InlineData("0123456789ABCDEF0123456789ABCDEF.png", false)]
    [InlineData("0123456789abcdef0123456789abcdef.jpeg", false)]
    [InlineData("appsettings.json", false)]
    public void Only_names_issued_by_the_storage_are_accepted_back(string name, bool expected) =>
        ImageUploadRules.IsStoredName(name).ShouldBe(expected);
}