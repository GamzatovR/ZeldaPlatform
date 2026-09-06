using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Common.Entities;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

/// <summary>Настройки приложения: естественный ключ-строка, суррогатного Id нет.</summary>
public sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSettings");

        builder.HasKey(setting => setting.Key);

        builder.Property(setting => setting.Key).HasMaxLength(120);
        builder.Property(setting => setting.Value).IsRequired();
    }
}