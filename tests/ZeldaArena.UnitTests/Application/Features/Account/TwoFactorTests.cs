using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Commands.DisableTwoFactor;
using ZeldaArena.Application.Features.Account.Commands.EnableTwoFactor;
using ZeldaArena.Application.Features.Account.Commands.SignInWithRecoveryCode;
using ZeldaArena.Application.Features.Account.Commands.SignInWithTwoFactor;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Account;

public class TwoFactorTests
{
    private const string Email = "player@zeldaarena.test";

    private readonly InMemoryUserAccountService _accounts = new();
    private readonly StubTwoFactorService _twoFactor = new();
    private readonly RecordingSignInService _signIn = new();

    [Fact]
    public async Task Enabling_returns_ten_recovery_codes()
    {
        var user = _accounts.Add(Email);

        var result = await Enable(user.Id, StubTwoFactorService.ValidCode);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Codes.Count.ShouldBe(EnableTwoFactorCommandHandler.RecoveryCodeCount);
        _twoFactor.IsEnabled(user.Id).ShouldBeTrue();
    }

    /// <summary>
    /// Cookie перевыписывается: в ней хранится признак прохождения второго фактора,
    /// и без обновления текущая сессия осталась бы в подвешенном состоянии.
    /// </summary>
    [Fact]
    public async Task Enabling_refreshes_the_current_session()
    {
        var user = _accounts.Add(Email);

        await Enable(user.Id, StubTwoFactorService.ValidCode);

        _signIn.RefreshedUsers.ShouldContain(user.Id);
    }

    /// <summary>
    /// Неверный код не включает второй фактор: иначе неправильно настроенный
    /// аутентификатор запер бы человека снаружи собственной учётной записи.
    /// </summary>
    [Fact]
    public async Task Wrong_verification_code_leaves_two_factor_off()
    {
        var user = _accounts.Add(Email);

        var result = await Enable(user.Id, "000000");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AccountErrors.TwoFactorCodeInvalid.Code);
        _twoFactor.IsEnabled(user.Id).ShouldBeFalse();
    }

    [Fact]
    public async Task Disabling_turns_it_off_and_resets_the_key()
    {
        var user = _accounts.Add(Email);
        await Enable(user.Id, StubTwoFactorService.ValidCode);

        var result = await Disable(user.Id);

        result.IsSuccess.ShouldBeTrue();
        _twoFactor.IsEnabled(user.Id).ShouldBeFalse();
        _twoFactor.WasKeyReset.ShouldBeTrue();
    }

    /// <summary>
    /// Прямое требование docs/SPEC.md §8.2: для роли Admin второй фактор обязателен.
    /// Проверка стоит в хендлере, а не только в разметке, — спрятанная кнопка
    /// эндпоинт не закрывает (§20, пункт 3).
    /// </summary>
    [Fact]
    public async Task Administrator_cannot_switch_two_factor_off()
    {
        var user = _accounts.Add(Email);
        await _accounts.AddToRoleAsync(user.Id, RoleNames.Admin);
        _twoFactor.Enable(user.Id);

        var result = await Disable(user.Id);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(DisableTwoFactorCommandHandler.AdminMustKeepTwoFactor.Code);
        _twoFactor.IsEnabled(user.Id).ShouldBeTrue();
    }

    [Fact]
    public async Task Correct_code_completes_the_sign_in()
    {
        var user = _accounts.Add(Email, twoFactorEnabled: true);
        _signIn.TwoFactorUser = user;
        _signIn.TwoFactorOutcome = SignInOutcome.Succeeded;

        var result = await SignInWithCode(rememberDevice: true);

        result.IsSuccess.ShouldBeTrue();
        _accounts.RecordedSignIns.ShouldBe([user.Id]);
        _signIn.RememberDeviceRequested.ShouldBeTrue();
    }

    /// <summary>
    /// Истёкшая промежуточная сессия и неверный код — разные исходы: в первом случае
    /// вход надо начинать с пароля, во втором достаточно повторить код.
    /// </summary>
    [Fact]
    public async Task Expired_intermediate_session_is_not_a_wrong_code()
    {
        _signIn.TwoFactorUser = null;

        var result = await SignInWithCode(rememberDevice: false);

        result.Error.Code.ShouldBe(AccountErrors.TwoFactorSessionExpired.Code);
    }

    [Fact]
    public async Task Wrong_code_does_not_record_a_sign_in()
    {
        var user = _accounts.Add(Email, twoFactorEnabled: true);
        _signIn.TwoFactorUser = user;
        _signIn.TwoFactorOutcome = SignInOutcome.Failed;

        var result = await SignInWithCode(rememberDevice: false);

        result.Error.Code.ShouldBe(AccountErrors.TwoFactorCodeInvalid.Code);
        _accounts.RecordedSignIns.ShouldBeEmpty();
    }

    [Fact]
    public async Task Recovery_code_completes_the_sign_in()
    {
        var user = _accounts.Add(Email, twoFactorEnabled: true);
        _signIn.TwoFactorUser = user;
        _signIn.RecoveryCodeOutcome = SignInOutcome.Succeeded;

        var result = await new SignInWithRecoveryCodeCommandHandler(_signIn, _accounts)
            .Handle(new SignInWithRecoveryCodeCommand("code-01"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _accounts.RecordedSignIns.ShouldBe([user.Id]);
    }

    [Fact]
    public async Task Used_recovery_code_is_refused()
    {
        var user = _accounts.Add(Email, twoFactorEnabled: true);
        _signIn.TwoFactorUser = user;
        _signIn.RecoveryCodeOutcome = SignInOutcome.Failed;

        var result = await new SignInWithRecoveryCodeCommandHandler(_signIn, _accounts)
            .Handle(new SignInWithRecoveryCodeCommand("code-01"), CancellationToken.None);

        result.Error.Code.ShouldBe(AccountErrors.RecoveryCodeInvalid.Code);
    }

    private Task<Result<RecoveryCodes>> Enable(Guid userId, string code) =>
        new EnableTwoFactorCommandHandler(
                new StubCurrentUserService { UserId = userId },
                _twoFactor,
                _signIn)
            .Handle(new EnableTwoFactorCommand(code), CancellationToken.None);

    private Task<Result> Disable(Guid userId) =>
        new DisableTwoFactorCommandHandler(
                new StubCurrentUserService { UserId = userId },
                _accounts,
                _twoFactor,
                _signIn)
            .Handle(new DisableTwoFactorCommand(), CancellationToken.None);

    private Task<Result> SignInWithCode(bool rememberDevice) =>
        new SignInWithTwoFactorCommandHandler(_signIn, _accounts)
            .Handle(
                new SignInWithTwoFactorCommand(
                    StubTwoFactorService.ValidCode,
                    RememberMe: false,
                    rememberDevice),
                CancellationToken.None);
}