using System.Text;

namespace ZeldaArena.Web.Extensions;

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