using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Учётные записи в памяти. Проверка пароля здесь честная только по совпадению строк —
/// хеширование это забота Identity, и подменять его в тестах хендлеров незачем.
/// </summary>
internal sealed class InMemoryUserAccountService : IUserAccountService
{
    private readonly Dictionary<Guid, UserAccountDto> _users = [];
    private readonly Dictionary<Guid, string> _passwords = [];

    /// <summary>Токены, выданные последними: тест проверяет, что письмо ушло именно с ними.</summary>
    public Dictionary<Guid, string> IssuedTokens { get; } = [];

    public List<Guid> RecordedSignIns { get; } = [];

    public List<Guid> InvalidatedSessions { get; } = [];

    /// <summary>Следующий вызов CreateAsync завершится этой ошибкой.</summary>
    public Error? CreateFailure { get; set; }

    public UserAccountDto Add(
        string email,
        string password = "Password-1234",
        bool emailConfirmed = true,
        bool isBlocked = false,
        bool twoFactorEnabled = false)
    {
        var user = new UserAccountDto(
            Guid.CreateVersion7(),
            email,
            email,
            emailConfirmed,
            twoFactorEnabled,
            isBlocked,
            IsLockedOut: false,
            DisplayName: null,
            AvatarPath: null,
            PreferredCulture: null,
            CountryCode: null,
            CreatedAt: DateTimeOffset.UnixEpoch,
            LastLoginAt: null,
            Roles: [RoleNames.User]);

        _users[user.Id] = user;
        _passwords[user.Id] = password;

        return user;
    }

    public Task<UserAccountDto?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.GetValueOrDefault(userId));

    public Task<UserAccountDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.Values.FirstOrDefault(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<Result<Guid>> CreateAsync(
        string email,
        string password,
        string? displayName,
        string? preferredCulture,
        CancellationToken cancellationToken = default)
    {
        if (CreateFailure is not null)
        {
            return Task.FromResult(Result.Failure<Guid>(CreateFailure));
        }

        if (_users.Values.Any(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(Result.Failure<Guid>(AccountErrors.EmailAlreadyTaken));
        }

        var created = Add(email, password, emailConfirmed: false);

        _users[created.Id] = created with
        {
            DisplayName = displayName,
            PreferredCulture = preferredCulture,
        };

        return Task.FromResult(Result.Success(created.Id));
    }

    public Task<Result> AddToRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default) =>
        Mutate(userId, user => user with { Roles = [.. user.Roles, role] });

    public Task<Result> RemoveFromRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default) =>
        Mutate(userId, user => user with
        {
            Roles = [.. user.Roles.Where(existing => existing != role)],
        });

    public Task<Result<string>> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        IssueToken(userId, "confirm");

    public Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default) =>
        IssuedTokens.GetValueOrDefault(userId) == token
            ? Mutate(userId, user => user with { EmailConfirmed = true })
            : Task.FromResult(Result.Failure(AccountErrors.InvalidToken));

    public Task<Result<string>> GeneratePasswordResetTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        IssueToken(userId, "reset");

    public Task<Result> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (IssuedTokens.GetValueOrDefault(userId) != token)
        {
            return Task.FromResult(Result.Failure(AccountErrors.InvalidToken));
        }

        _passwords[userId] = newPassword;

        return Task.FromResult(Result.Success());
    }

    public Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (!_passwords.TryGetValue(userId, out var actual))
        {
            return Task.FromResult(Result.Failure(AccountErrors.UserNotFound));
        }

        if (actual != currentPassword)
        {
            return Task.FromResult(Result.Failure(AccountErrors.IncorrectPassword));
        }

        _passwords[userId] = newPassword;

        return Task.FromResult(Result.Success());
    }

    public Task<Result<string>> GenerateEmailChangeTokenAsync(
        Guid userId,
        string newEmail,
        CancellationToken cancellationToken = default) =>
        IssueToken(userId, $"change:{newEmail}");

    public Task<Result> ChangeEmailAsync(
        Guid userId,
        string newEmail,
        string token,
        CancellationToken cancellationToken = default) =>
        IssuedTokens.GetValueOrDefault(userId) == token
            ? Mutate(userId, user => user with { Email = newEmail, UserName = newEmail })
            : Task.FromResult(Result.Failure(AccountErrors.InvalidToken));

    public Task<Result> UpdateProfileAsync(
        Guid userId,
        string? displayName,
        string? countryCode,
        string? preferredCulture,
        CancellationToken cancellationToken = default) =>
        Mutate(userId, user => user with
        {
            DisplayName = displayName,
            CountryCode = countryCode,
            PreferredCulture = preferredCulture,
        });

    public Task<Result> RecordSignInAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        RecordedSignIns.Add(userId);

        return Task.FromResult(Result.Success());
    }

    public Task<Result> InvalidateOtherSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        InvalidatedSessions.Add(userId);

        return Task.FromResult(Result.Success());
    }

    public string PasswordOf(Guid userId) => _passwords[userId];

    private Task<Result<string>> IssueToken(Guid userId, string prefix)
    {
        if (!_users.ContainsKey(userId))
        {
            return Task.FromResult(Result.Failure<string>(AccountErrors.UserNotFound));
        }

        var token = $"{prefix}:{Guid.CreateVersion7()}";
        IssuedTokens[userId] = token;

        return Task.FromResult(Result.Success(token));
    }

    private Task<Result> Mutate(Guid userId, Func<UserAccountDto, UserAccountDto> change)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            return Task.FromResult(Result.Failure(AccountErrors.UserNotFound));
        }

        _users[userId] = change(user);

        return Task.FromResult(Result.Success());
    }
}