using System.Text;

namespace ZeldaArena.Web.Extensions;

/// <summary>
/// Ключ ресурса для значения перечисления: <c>MatchStatus.Live</c> → <c>enum.match_status.live</c>.
///
/// Названия статусов, ролей и регионов локализуются (docs/SPEC.md §9.5), а вывести
/// <c>@Model.Status</c> как есть значило бы показать пользователю идентификатор из кода.
/// Ключ строится по одному правилу, а не перечисляется вручную: новое значение
/// перечисления без перевода проявится в разметке своим ключом, а не пустым местом.
/// </summary>
public static class EnumResourceKeys
{
    public static string For<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        $"enum.{SnakeCase(typeof(TEnum).Name)}.{SnakeCase(value.ToString())}";

    private static string SnakeCase(string name)
    {
        var builder = new StringBuilder(name.Length + 4);

        for (var index = 0; index < name.Length; index++)
        {
            var symbol = name[index];

            if (char.IsUpper(symbol) && index > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(symbol));
        }

        return builder.ToString();
    }
}