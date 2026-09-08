using ZeldaArena.Application.Common.Behaviors;

namespace ZeldaArena.UnitTests.Application.Common.Behaviors;

/// <summary>
/// Список запрещённых к записи полей — прямое требование docs/SPEC.md §13 и §20 пункт 6.
/// Проверка идёт по именам, которыми поля реально названы в командах: список фрагментов
/// легко пополнить и так же легко забыть, что новое имя под него не подошло.
/// </summary>
public class SensitivePropertiesTests
{
    [Theory]
    [InlineData("Password")]
    [InlineData("NewPassword")]
    [InlineData("CurrentPassword")]
    [InlineData("ConfirmPassword")]
    [InlineData("Token")]
    [InlineData("ResetToken")]
    [InlineData("CardNumber")]
    [InlineData("Cvv")]
    [InlineData("ConfirmationCode")]
    [InlineData("RecoveryCode")]
    [InlineData("RecoveryCodes")]
    [InlineData("TwoFactorCode")]
    [InlineData("SharedKey")]
    [InlineData("AuthenticatorKey")]
    [InlineData("AuthenticatorUri")]
    public void Secret_field_names_are_masked(string propertyName) =>
        SensitiveProperties.IsSensitive(propertyName).ShouldBeTrue();

    /// <summary>
    /// Обратная сторона: под маску не должно попадать то, ради чего аудит и ведётся.
    /// Коды фич и тарифов в записи нужны — по ним на защите показывается EP-3/EP-4.
    /// </summary>
    [Theory]
    [InlineData("FeatureCode")]
    [InlineData("PlanCode")]
    [InlineData("Email")]
    [InlineData("DisplayName")]
    [InlineData("MatchId")]
    public void Ordinary_field_names_stay_readable(string propertyName) =>
        SensitiveProperties.IsSensitive(propertyName).ShouldBeFalse();
}