using Fediva.Application.Ports.Clock;
using Fediva.Application.Ports.Identity;
using Fediva.Application.Ports.Persistence;
using Fediva.Domain.Common;
using Fediva.Domain.Identity.Users;

namespace Fediva.Application.Identity.Commands;

/// <summary>
/// ユーザー登録コマンド
/// </summary>
/// <param name="DisplayName">表示名</param>
/// <param name="EmailAddress">メールアドレス</param>
/// <param name="Password">パスワード</param>
public sealed record RegisterUserCommand(
    DisplayName DisplayName,
    EmailAddress EmailAddress,
    string Password
) : ICommand<UserId>;

/// <summary>
/// ユーザー登録コマンドハンドラー
/// </summary>
public sealed class RegisterUserHandler(
    IAccountService accountService,
    IUserRepository userRepository,
    IClock clock
) : ICommandHandler<RegisterUserCommand, UserId>
{
    public async Task<Result<UserId>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        var userId = UserId.New();
        var accountResult = await accountService.RegisterAsync(
            userId,
            command.EmailAddress,
            command.Password,
            cancellationToken
        );

        if (accountResult.IsFailure)
        {
            return Result<UserId>.Failure(accountResult.Error);
        }

        try
        {
            var user = User.Create(userId, command.DisplayName, command.EmailAddress, clock.UtcNow);
            await userRepository.AddAsync(user, cancellationToken);

            return Result<UserId>.Success(userId);
        }
        catch (Exception)
        {
            // ドメインストアへのユーザー保存失敗時に、作成済みのアカウントを補償削除して不整合を防ぐ。
            await accountService.DeleteAsync(userId, CancellationToken.None);
            throw;
        }
    }
}
