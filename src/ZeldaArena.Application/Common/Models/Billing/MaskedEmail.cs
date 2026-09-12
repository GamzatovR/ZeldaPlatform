namespace ZeldaArena.Application.Common.Models.Billing;

public static class MaskedEmail
{
    public static string Of(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var value = email.Trim();
        var at = value.IndexOf('@', StringComparison.Ordinal);

        // Адрес без «собаки» сюда не попадёт — его отвергает валидатор, — но
        // маскировщик не должен ронять страницу, если это когда-нибудь случится.
        if (at < 1)
        {
            return "***";
        }

        var name = value[..at];
        var domain = value[at..];

        return name.Length == 1
            ? $"{name}***{domain}"
            : $"{name[0]}***{name[^1]}{domain}";
    }
}