using Fediva.Domain.Common;
using Fediva.Domain.Identity.Users;

namespace Fediva.Application.Ports.Identity;

/// <summary>
/// 外部認証・アカウント管理システムとの連携を行うポート
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// 新規ユーザーアカウントを認証ストアに登録します。
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="emailAddress">メールアドレス</param>
    /// <param name="password">パスワード</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>登録結果を表す <see cref="Result"/>。</returns>
    Task<Result> RegisterAsync(
        UserId userId,
        EmailAddress emailAddress,
        string password,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// アカウントのパスワードを変更します。
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="currentPassword">現在のパスワード</param>
    /// <param name="newPassword">新しいパスワード</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>変更結果を表す <see cref="Result"/>。</returns>
    Task<Result> ChangePasswordAsync(
        UserId userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// アカウントを削除します。
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>削除結果を表す <see cref="Result"/>。</returns>
    Task<Result> DeleteAsync(UserId userId, CancellationToken cancellationToken = default);
}
