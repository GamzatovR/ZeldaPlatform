using ZeldaArena.Application.Features.Files.Queries.GetStoredFile;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Files;

public class GetStoredFileQueryHandlerTests
{
    private readonly InMemoryFileStorage _storage = new();

    [Fact]
    public async Task Stored_image_is_served_with_the_type_of_its_real_format()
    {
        var stored = await _storage.SaveAsync(SampleImages.Webp.AsStream(), "logo.png", "image/png");

        var file = await Handle(stored.StoredPath);

        file.ShouldNotBeNull();
        file.ContentType.ShouldBe("image/webp");
    }

    [Fact]
    public async Task Unknown_file_is_not_found() =>
        (await Handle("0123456789abcdef0123456789abcdef.png")).ShouldBeNull();

    /// <summary>
    /// Чужое имя — «файла нет», а не исключение: иначе опечатка в ссылке
    /// оборачивалась бы ошибкой сервера.
    /// </summary>
    [Theory]
    [InlineData("../appsettings.json")]
    [InlineData("..%2Fappsettings.json")]
    [InlineData("logo.png")]
    public async Task Name_not_issued_by_the_storage_is_not_found(string name) =>
        (await Handle(name)).ShouldBeNull();

    private Task<StoredFileContent?> Handle(string name) =>
        new GetStoredFileQueryHandler(_storage).Handle(new GetStoredFileQuery(name), CancellationToken.None);
}