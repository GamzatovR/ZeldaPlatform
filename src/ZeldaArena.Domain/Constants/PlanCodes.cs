namespace ZeldaArena.Domain.Constants;

/// <summary>
/// Коды тарифов из сида. Код нужен, чтобы сид был идемпотентным и чтобы тариф
/// можно было найти по устойчивому ключу; правами тариф не управляет — права даёт
/// только набор фич (docs/SPEC.md §8.1).
/// </summary>
public static class PlanCodes
{
    public const string Free = "free";

    public const string ProMonth = "pro-month";

    public const string ProYear = "pro-year";
}