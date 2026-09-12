using ZeldaArena.Application.Common.Behaviors;

namespace ZeldaArena.UnitTests.Application.Common.Behaviors;

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

    [Theory]
    [InlineData("FeatureCode")]
    [InlineData("PlanCode")]
    [InlineData("Email")]
    [InlineData("DisplayName")]
    [InlineData("MatchId")]
    public void Ordinary_field_names_stay_readable(string propertyName) =>
        SensitiveProperties.IsSensitive(propertyName).ShouldBeFalse();
}