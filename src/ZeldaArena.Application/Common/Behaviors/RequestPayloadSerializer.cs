using System.Reflection;
using System.Text.Json;

using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Common.Behaviors;

/// <summary>Готовит содержимое команды для записи в аудит.</summary>
internal static class RequestPayloadSerializer
{
    private static readonly HashSet<string> ContractProperties = new(StringComparer.Ordinal)
    {
        nameof(IAuditableRequest.AuditEntityType),
        nameof(IAuditableRequest.AuditEntityId),
    };

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
            .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
            .Where(property => !ContractProperties.Contains(property.Name));

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