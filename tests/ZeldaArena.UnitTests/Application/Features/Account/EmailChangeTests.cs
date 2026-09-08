using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Commands.ConfirmEmailChange;
using ZeldaArena.Application.Features.Account.Commands.RequestEmailChange;
using ZeldaArena.Domain.Common;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Account;

public class EmailChangeTests
{
    private const string CurrentEmail = "player@zeldaarena.test";
    private const string NewEmail = "new-player@zeldaarena.test";

    private readonly InMemoryUserAccountService _accounts = new();
    private readonly RecordingAccountEmailSender _email = new();

    /// <summary>
    /// Два письма — прямое требование docs/SPEC.md §8.2: ссылка уходит на новый адрес,
    /// уведомление на старый, чтобы захват учётной записи не прошёл незамеченным.
    /// </summary>
    [Fact]
    public async Task Request_sends_a_link_to_the_new_address_and_a_notice_to_the_old_one()
    {
        var user = _accounts.Add(CurrentEmail);

        var result = await Request(user.Id, NewEmail);

        result.IsSuccess.ShouldBeTrue();
        _email.Single(RecordingAccountEmailSender.LetterKind.EmailChangeConfirmation)
            .To.ShouldBe(NewEmail);
        _email.Single(RecordingAccountEmailSender.LetterKind.EmailChangedNotice)
            .To.ShouldBe(CurrentEmail);
    }

    /// <summary>До подтверждения адрес учётной записи остаётся прежним.</summary>
    [Fact]
    public async Task Request_alone_does_not_change_the_address()
    {
        var user = _accounts.Add(CurrentEmail);

        await Request(user.Id, NewEmail);

        (await _accounts.FindByIdAsync(user.Id)).ShouldNotBeNull().Email.ShouldBe(CurrentEmail);
    }

    [Fact]
    public async Task Address_already_taken_is_refused()
    {
        var user = _accounts.Add(CurrentEmail);
        _accounts.Add(NewEmail);

        var result = await Request(user.Id, NewEmail);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AccountErrors.EmailAlreadyTaken.Code);
        _email.Sent.ShouldBeEmpty();
    }

    /// <summary>Смена на тот же адрес — не ошибка, но и писем по ней слать незачем.</summary>
    [Fact]
    public async Task Changing_to_the_same_address_sends_nothing()
    {
        var user = _accounts.Add(CurrentEmail);

        var result = await Request(user.Id, CurrentEmail.ToUpperInvariant());

        result.IsSuccess.ShouldBeTrue();
        _email.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task Confirmation_applies_the_new_address_and_ends_other_sessions()
    {
        var user = _accounts.Add(CurrentEmail);
        await Request(user.Id, NewEmail);

        var token = _accounts.IssuedTokens[user.Id];
        var result = await Confirm(user.Id, NewEmail, token);

        result.IsSuccess.ShouldBeTrue();
        (await _accounts.FindByIdAsync(user.Id)).ShouldNotBeNull().Email.ShouldBe(NewEmail);
        _accounts.InvalidatedSessions.ShouldContain(user.Id);
    }

    [Fact]
    public async Task Tampered_token_leaves_the_address_alone()
    {
        var user = _accounts.Add(CurrentEmail);
        await Request(user.Id, NewEmail);

        var result = await Confirm(user.Id, NewEmail, "подделка");

        result.IsFailure.ShouldBeTrue();
        (await _accounts.FindByIdAsync(user.Id)).ShouldNotBeNull().Email.ShouldBe(CurrentEmail);
    }

    private Task<Result> Request(Guid userId, string newEmail) =>
        new RequestEmailChangeCommandHandler(
                new StubCurrentUserService { UserId = userId },
                _accounts,
                _email)
            .Handle(new RequestEmailChangeCommand(newEmail), CancellationToken.None);

    private Task<Result> Confirm(Guid userId, string newEmail, string token) =>
        new ConfirmEmailChangeCommandHandler(_accounts)
            .Handle(new ConfirmEmailChangeCommand(userId, newEmail, token), CancellationToken.None);
}