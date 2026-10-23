using Fediva.Application.Ports.Identity;
using Fediva.Domain.Common;

namespace Fediva.Application.Identity.Commands;

/// <summary>
/// パスワード変更コマンド
/// </summary>
/// <param name="CurrentPassword">現在のパスワード</param>
/// <param name="NewPassword">新しいパスワード</param>
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand;

/// <summary>
/// パスワード変更コマンドハンドラー
/// </summary>
public sealed class ChangePasswordHandler(IAccountService accountService, ICurrentUser currentUser)
    : ICommandHandler<ChangePasswordCommand>
{
    public Task<Result> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        if (currentUser.Id is not { } userId)
        {
            return Task.FromResult(Result.Failure(ApplicationErrors.CurrentUserRequired));
        }

        return accountService.ChangePasswordAsync(
            userId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken
        );
    }
}
