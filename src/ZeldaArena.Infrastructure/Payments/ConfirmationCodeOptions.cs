namespace ZeldaArena.Infrastructure.Payments;

public sealed class ConfirmationCodeOptions
{
    public const string SectionName = "Payments:ConfirmationCode";

    public string Pepper { get; set; } = string.Empty;
}