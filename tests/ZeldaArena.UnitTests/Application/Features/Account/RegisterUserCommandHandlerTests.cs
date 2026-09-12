using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Commands.RegisterUser;
using ZeldaArena.Domain.Common;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Account;

public class RegisterUserCommandHandlerTests
{
    private readonly InMemoryUserAccountService _accounts = new();
    private readonly RecordingAccountEmailSender _email = new();

    [Fact]
    public async Task Registration_creates_the_account_and_sends_a_confirmation_letter()
    {
        var result = await Handle(Command());

        result.IsSuccess.ShouldBeTrue();

        var user = await _accounts.FindByEmailAsync("player@zeldaarena.test");
        user.ShouldNotBeNull();

        var letter = _email.Single(RecordingAccountEmailSender.LetterKind.EmailConfirmation);
        letter.To.ShouldBe("player@zeldaarena.test");
        letter.UserId.ShouldBe(user.Id);
    }

    [Fact]
    public async Task New_account_starts_unconfirmed()
    {
        await Handle(Command());

        var user = await _accounts.FindByEmailAsync("player@zeldaarena.test");

        user.ShouldNotBeNull().EmailConfirmed.ShouldBeFalse();
    }

    /// <summary>Токен из письма должен быть тем самым, который выдал сервис учётных записей.</summary>
    [Fact]
    public async Task Confirmation_letter_carries_the_issued_token()
    {
        await Handle(Command());

        var user = await _accounts.FindByEmailAsync("player@zeldaarena.test");
        var letter = _email.Single(RecordingAccountEmailSender.LetterKind.EmailConfirmation);

        letter.Token.ShouldBe(_accounts.IssuedTokens[user.ShouldNotBeNull().Id]);
    }

    [Fact]
    public async Task Taken_email_is_rejected_and_no_letter_is_sent()
    {
        _accounts.Add("player@zeldaarena.test");

        var result = await Handle(Command());

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AccountErrors.EmailAlreadyTaken.Code);
        _email.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task Broken_mail_server_fails_the_whole_registration()
    {
        _email.FailWith = new InvalidOperationException("SMTP недоступен");

        await Should.ThrowAsync<InvalidOperationException>(() => Handle(Command()));
    }

    private static RegisterUserCommand Command() =>
        new("player@zeldaarena.test", "Password-1234", "Ganondorf", "ru");

    private Task<Result> Handle(RegisterUserCommand command) =>
        new RegisterUserCommandHandler(_accounts, _email).Handle(command, CancellationToken.None);
}