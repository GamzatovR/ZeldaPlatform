using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Commands.ChangePassword;
using ZeldaArena.Application.Features.Account.Commands.ForgotPassword;
using ZeldaArena.Application.Features.Account.Commands.ResetPassword;
using ZeldaArena.Domain.Common;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Account;

public class PasswordRecoveryTests
{
    private const string Email = "player@zeldaarena.test";
    private const string OldPassword = "Password-1234";
    private const string NewPassword = "Password-5678";

    private readonly InMemoryUserAccountService _accounts = new();
    private readonly RecordingAccountEmailSender _email = new();
    private readonly RecordingSignInService _signIn = new();

    [Fact]
    public async Task Forgot_password_sends_a_reset_link()
    {
        var user = _accounts.Add(Email);

        var result = await Forgot(Email);

        result.IsSuccess.ShouldBeTrue();
        _email.Single(RecordingAccountEmailSender.LetterKind.PasswordReset)
            .Token.ShouldBe(_accounts.IssuedTokens[user.Id]);
    }

    /// <summary>
    /// Прямое требование docs/SPEC.md §8.2: ответ формы одинаков независимо от того,
    /// заведён адрес или нет. Иначе форму используют для перебора адресов.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Forgot_password_answers_the_same_for_any_address(bool accountExists)
    {
        if (accountExists)
        {
            _accounts.Add("someone-else@zeldaarena.test");
        }

        var result = await Forgot(Email);

        result.IsSuccess.ShouldBeTrue();
        _email.Sent.ShouldBeEmpty();
    }

    /// <summary>
    /// Сброс пароля не должен обходить подтверждение почты: иначе на чужой
    /// неподтверждённый адрес можно было бы завести учётную запись и тут же захватить её.
    /// </summary>
    [Fact]
    public async Task Unconfirmed_address_gets_no_reset_link()
    {
        _accounts.Add(Email, emailConfirmed: false);

        var result = await Forgot(Email);

        result.IsSuccess.ShouldBeTrue();
        _email.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task Reset_sets_the_new_password_and_ends_other_sessions()
    {
        var user = _accounts.Add(Email);
        var token = (await _accounts.GeneratePasswordResetTokenAsync(user.Id)).Value;

        var result = await Reset(user.Id, token);

        result.IsSuccess.ShouldBeTrue();
        _accounts.PasswordOf(user.Id).ShouldBe(NewPassword);
        _accounts.InvalidatedSessions.ShouldContain(user.Id);
        _email.Contains(RecordingAccountEmailSender.LetterKind.PasswordChangedNotice).ShouldBeTrue();
    }

    [Fact]
    public async Task Reset_with_a_tampered_token_changes_nothing()
    {
        var user = _accounts.Add(Email);
        await _accounts.GeneratePasswordResetTokenAsync(user.Id);

        var result = await Reset(user.Id, "подделка");

        result.IsFailure.ShouldBeTrue();
        _accounts.PasswordOf(user.Id).ShouldBe(OldPassword);
        _accounts.InvalidatedSessions.ShouldBeEmpty();
    }

    /// <summary>
    /// Несуществующий пользователь отвечает так же, как испорченный токен: по ответу
    /// нельзя проверить, есть ли учётная запись с таким идентификатором.
    /// </summary>
    [Fact]
    public async Task Reset_for_an_unknown_user_looks_like_a_bad_token()
    {
        var result = await Reset(Guid.CreateVersion7(), "reset:whatever");

        result.Error.Code.ShouldBe(AccountErrors.InvalidToken.Code);
    }

    [Fact]
    public async Task Change_password_requires_the_current_one()
    {
        var user = _accounts.Add(Email);

        var result = await Change(user.Id, currentPassword: "Not-The-Password-1");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AccountErrors.IncorrectPassword.Code);
        _accounts.PasswordOf(user.Id).ShouldBe(OldPassword);
    }

    /// <summary>
    /// После смены пароля остальные сессии закрываются, а текущая переподписывается:
    /// иначе человек разлогинил бы сам себя (docs/SPEC.md §8.2).
    /// </summary>
    [Fact]
    public async Task Change_password_ends_other_sessions_but_keeps_the_current_one()
    {
        var user = _accounts.Add(Email);

        var result = await Change(user.Id, OldPassword);

        result.IsSuccess.ShouldBeTrue();
        _accounts.PasswordOf(user.Id).ShouldBe(NewPassword);
        _accounts.InvalidatedSessions.ShouldContain(user.Id);
        _signIn.RefreshedUsers.ShouldContain(user.Id);
        _email.Contains(RecordingAccountEmailSender.LetterKind.PasswordChangedNotice).ShouldBeTrue();
    }

    [Fact]
    public async Task Change_password_without_a_signed_in_user_is_refused()
    {
        var result = await new ChangePasswordCommandHandler(
                new StubCurrentUserService(),
                _accounts,
                _signIn,
                _email)
            .Handle(new ChangePasswordCommand(OldPassword, NewPassword), CancellationToken.None);

        result.Error.Code.ShouldBe(AccountErrors.UserNotFound.Code);
    }

    private Task<Result> Forgot(string email) =>
        new ForgotPasswordCommandHandler(_accounts, _email)
            .Handle(new ForgotPasswordCommand(email), CancellationToken.None);

    private Task<Result> Reset(Guid userId, string token) =>
        new ResetPasswordCommandHandler(_accounts, _email)
            .Handle(new ResetPasswordCommand(userId, token, NewPassword), CancellationToken.None);

    private Task<Result> Change(Guid userId, string currentPassword) =>
        new ChangePasswordCommandHandler(
                new StubCurrentUserService { UserId = userId },
                _accounts,
                _signIn,
                _email)
            .Handle(new ChangePasswordCommand(currentPassword, NewPassword), CancellationToken.None);
}