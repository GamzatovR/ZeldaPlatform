namespace ZeldaArena.Application.Common.Validation;

/// <summary>
/// Разбор и проверка номера банковской карты (docs/SPEC.md §7.6, шаг 1).
///
/// Живёт в Application, а не в реализации платёжного провайдера, потому что нужен
/// обоим: валидатор отвергает негодный номер до хендлера и объясняет, что не так,
/// а провайдер проверяет то же самое ещё раз — его могут вызвать и в обход формы.
/// Две копии алгоритма разъехались бы, и форма начала бы принимать номера,
/// которые провайдер отклоняет.
///
/// Сам номер здесь только проверяется и никуда не сохраняется.
/// </summary>
public static class CardNumber
{
    /// <summary>Границы длины по ISO/IEC 7812.</summary>
    public const int MinLength = 13;

    public const int MaxLength = 19;

    /// <summary>Длина CVV: три цифры (docs/SPEC.md §7.6).</summary>
    public const int CvvLength = 3;

    /// <summary>Пробелы и дефисы, которыми пользователи разбивают номер, отбрасываются.</summary>
    public static string OnlyDigits(string? value) =>
        value is null ? string.Empty : new string([.. value.Where(char.IsAsciiDigit)]);

    /// <summary>
    /// Алгоритм Луна: контрольная сумма, которой обладает любой настоящий номер карты.
    /// От опечатки защищает, от выдумки — нет, и для мнимой оплаты этого достаточно.
    /// </summary>
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