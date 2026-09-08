using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Commands.ConfirmEmail;
using ZeldaArena.Application.Features.Account.Commands.ResendEmailConfirmation;
using ZeldaArena.Domain.Common;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Account;

public class EmailConfirmationTests
{
    private const string Email = "player@zeldaarena.test";

    private readonly InMemoryUserAccountService _accounts = new();
    private readonly RecordingAccountEmailSender _email = new();

    [Fact]
    public async Task Valid_link_confirms_the_address()
    {
        var user = _accounts.Add(Email, emailConfirmed: false);
        var token = (await _accounts.GenerateEmailConfirmationTokenAsync(user.Id)).Value;

        var result = await Confirm(user.Id, token);

        result.IsSuccess.ShouldBeTrue();
        (await _accounts.FindByIdAsync(user.Id)).ShouldNotBeNull().EmailConfirmed.ShouldBeTrue();
    }

    [Fact]
    public async Task Tampered_token_is_refused()
    {
        var user = _accounts.Add(Email, emailConfirmed: false);
        await _accounts.GenerateEmailConfirmationTokenAsync(user.Id);

        var result = await Confirm(user.Id, "подделка");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AccountErrors.InvalidToken.Code);
    }

    [Fact]
    public async Task Link_for_a_deleted_account_is_refused()
    {
        var result = await Confirm(Guid.CreateVersion7(), "confirm:whatever");

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Resend_sends_a_fresh_letter_to_an_unconfirmed_address()
    {
        var user = _accounts.Add(Email, emailConfirmed: false);

        var result = await Resend(Email);

        result.IsSuccess.ShouldBeTrue();
        _email.Single(RecordingAccountEmailSender.LetterKind.EmailConfirmation)
            .Token.ShouldBe(_accounts.IssuedTokens[user.Id]);
    }

    /// <summary>
    /// Ответ одинаков для любого адреса, иначе форма показывала бы, какие адреса
    /// заведены на портале (docs/SPEC.md §8.2). Письмо при этом не уходит.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Resend_never_reveals_whether_the_address_is_known(bool accountExists)
    {
        if (accountExists)
        {
            _accounts.Add(Email, emailConfirmed: true);
        }

        var result = await Resend(Email);

        result.IsSuccess.ShouldBeTrue();
        _email.Sent.ShouldBeEmpty();
    }

    private Task<Result> Confirm(Guid userId, string token) =>
        new ConfirmEmailCommandHandler(_accounts)
            .Handle(new ConfirmEmailCommand(userId, token), CancellationToken.None);

    private Task<Result> Resend(string email) =>
        new ResendEmailConfirmationCommandHandler(_accounts, _email)
            .Handle(new ResendEmailConfirmationCommand(email), CancellationToken.None);
}