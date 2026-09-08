using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Commands.SignIn;
using ZeldaArena.Domain.Common;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Account;

public class SignInCommandHandlerTests
{
    private const string Email = "player@zeldaarena.test";

    private readonly InMemoryUserAccountService _accounts = new();
    private readonly RecordingSignInService _signIn = new();

    [Fact]
    public async Task Successful_sign_in_returns_succeeded_and_is_recorded()
    {
        var user = _accounts.Add(Email);
        _signIn.PasswordOutcome = SignInOutcome.Succeeded;

        var result = await Handle();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(SignInOutcome.Succeeded);
        _accounts.RecordedSignIns.ShouldBe([user.Id]);
    }

    /// <summary>
    /// «Нужен второй фактор» — успех сценария, а не отказ: пароль подошёл, и страница
    /// обязана перейти к вводу кода, а не показать ошибку (docs/SPEC.md §8.2).
    /// </summary>
    [Fact]
    public async Task Two_factor_requirement_is_a_success()
    {
        _accounts.Add(Email);
        _signIn.PasswordOutcome = SignInOutcome.RequiresTwoFactor;

        var result = await Handle();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(SignInOutcome.RequiresTwoFactor);
    }

    /// <summary>
    /// Второй фактор ещё не пройден, значит вход не состоялся: отметку о нём ставить рано.
    /// </summary>
    [Fact]
    public async Task Two_factor_requirement_does_not_record_a_sign_in()
    {
        _accounts.Add(Email);
        _signIn.PasswordOutcome = SignInOutcome.RequiresTwoFactor;

        await Handle();

        _accounts.RecordedSignIns.ShouldBeEmpty();
    }

    /// <summary>
    /// Неудача возвращается через Result, а не исключением, — только так AuditBehavior
    /// запишет неудачный вход в журнал, чего прямо требует §8.2.
    /// </summary>
    [Theory]
    [InlineData(SignInOutcome.Failed, "account.invalid_credentials")]
    [InlineData(SignInOutcome.LockedOut, "account.locked_out")]
    [InlineData(SignInOutcome.NotAllowed, "account.email_not_confirmed")]
    public async Task Unsuccessful_outcomes_become_failures(SignInOutcome outcome, string errorCode)
    {
        _accounts.Add(Email);
        _signIn.PasswordOutcome = outcome;

        var result = await Handle();

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(errorCode);
    }

    /// <summary>
    /// Ответ на незнакомый адрес неотличим от ответа на неверный пароль: иначе форма
    /// входа становится средством перебора заведённых адресов.
    /// </summary>
    [Fact]
    public async Task Unknown_email_looks_exactly_like_a_wrong_password()
    {
        _signIn.PasswordOutcome = SignInOutcome.Failed;

        var result = await Handle();

        result.Error.Code.ShouldBe(AccountErrors.InvalidCredentials.Code);
    }

    /// <summary>
    /// Заблокированный администратором не получает cookie даже на время одного запроса:
    /// проверка идёт до обращения к SignInManager.
    /// </summary>
    [Fact]
    public async Task Blocked_user_is_refused_before_the_password_is_checked()
    {
        _accounts.Add(Email, isBlocked: true);
        _signIn.PasswordOutcome = SignInOutcome.Succeeded;

        var result = await Handle();

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AccountErrors.Blocked.Code);
        _accounts.RecordedSignIns.ShouldBeEmpty();
    }

    private Task<Result<SignInOutcome>> Handle() =>
        new SignInCommandHandler(_signIn, _accounts)
            .Handle(new SignInCommand(Email, "Password-1234", RememberMe: false), CancellationToken.None);
}