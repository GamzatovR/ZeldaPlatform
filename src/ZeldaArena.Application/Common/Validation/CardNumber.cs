namespace ZeldaArena.Application.Common.Validation;

public static class CardNumber
{
    /// <summary>Границы длины по ISO/IEC 7812.</summary>
    public const int MinLength = 13;

    public const int MaxLength = 19;

    /// <summary>Длина CVV: три цифры.</summary>
    public const int CvvLength = 3;

    /// <summary>Пробелы и дефисы, которыми пользователи разбивают номер, отбрасываются.</summary>
    public static string OnlyDigits(string? value) =>
        value is null ? string.Empty : new string([.. value.Where(char.IsAsciiDigit)]);

    public static bool PassesLuhn(string? value)
    {
        var digits = OnlyDigits(value);

        if (digits.Length is < MinLength or > MaxLength)
        {
            return false;
        }

        var sum = 0;
        var doubled = false;

        for (var index = digits.Length - 1; index >= 0; index--)
        {
            var digit = digits[index] - '0';

            if (doubled)
            {
                digit *= 2;

                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            doubled = !doubled;
        }

        return sum % 10 == 0;
    }
}