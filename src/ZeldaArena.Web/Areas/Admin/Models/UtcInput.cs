namespace ZeldaArena.Web.Areas.Admin.Models;

public static class UtcInput
{
    public static DateTimeOffset ToUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), TimeSpan.Zero);

    public static DateTime FromUtc(DateTimeOffset value) =>
        DateTime.SpecifyKind(value.UtcDateTime, DateTimeKind.Unspecified);
}