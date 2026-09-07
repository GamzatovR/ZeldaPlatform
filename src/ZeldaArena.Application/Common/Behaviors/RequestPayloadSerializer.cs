using System.Reflection;
using System.Text.Json;

namespace ZeldaArena.Application.Common.Behaviors;

/// <summary>
/// Готовит содержимое команды для записи в аудит. Сериализуется не сам объект,
/// а словарь его публичных свойств, из которого предварительно вычеркнуты секреты
/// по <see cref="SensitiveProperties"/>.
///
/// Отдельный проход по свойствам, а не атрибут <c>[JsonIgnore]</c> на командах:
/// атрибут нужно не забыть поставить, а забытый атрибут — это номер карты в аудите
/// (docs/SPEC.md §20 пункт 6). Умолчание должно быть безопасным.
/// </summary>
internal static class RequestPayloadSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string? ToJson(object request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var properties = request.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.GetIndexParameters().Length == 0);

        var payload = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var property in properties)
        {
            payload[property.Name] = SensitiveProperties.IsSensitive(property.Name)
                ? SensitiveProperties.Mask
                : ReadValue(property, request);
        }

        return payload.Count == 0 ? null : JsonSerializer.Serialize(payload, Options);
    }

    private static object? ReadValue(PropertyInfo property, object request)
    {
        // Вычисляемое свойство может бросить исключение (например, Result.Value
        // у неудачного результата). Аудит не должен ронять команду из-за этого.
        try
        {
            var value = property.GetValue(request);

            // Строковое представление, а не сам объект: сериализация чужого графа
            // может утащить в аудит лишнее и зациклиться на навигационных свойствах.
            return value switch
            {
                null => null,
                string or bool or Guid => value,
                _ when property.PropertyType.IsPrimitive || property.PropertyType.IsEnum => value.ToString(),
                _ => value.ToString(),
            };
        }
        catch (TargetInvocationException)
        {
            return null;
        }
    }
}