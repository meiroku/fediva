using Fediva.Domain.Identity.Users;

namespace Fediva.Application.Ports.Persistence;

/// <summary>
/// <see cref="User"/> ドメイン集約の永続化・再構築を行うリポジトリポート
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 指定されたIDのユーザーを取得します。
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>存在する場合は <see cref="User"/>、存在しない場合は <c>null</c>。</returns>
    Task<User?> FindByIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新規ユーザーを永続化ストアに追加します。
    /// </summary>
    /// <param name="user">追加するユーザー集約</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// 既存ユーザーの変更を永続化ストアに反映します。
    /// </summary>
    /// <param name="user">更新するユーザー集約</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
