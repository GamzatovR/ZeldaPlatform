namespace ZeldaArena.Domain.Common.Entities;

/// <summary>Настройка приложения, редактируемая из админки.</summary>
public class AppSetting
{
    private AppSetting()
    {
    }

    public string Key { get; private set; } = null!;

    public string Value { get; private set; } = null!;

    public DateTimeOffset UpdatedAt { get; private set; }

    public static AppSetting Create(string key, string value, DateTimeOffset updatedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        return new AppSetting
        {
            Key = key.Trim(),
            Value = value,
            UpdatedAt = updatedAt,
        };
    }

    public void SetValue(string value, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
        UpdatedAt = updatedAt;
    }
}