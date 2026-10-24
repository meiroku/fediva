namespace Fediva.Application.Ports.Persistence;

/// <summary>
/// 作業単位およびトランザクション境界を表すポート
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 現在の作業単位で保留中の変更を永続化ストアに保存します。
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>永続化ストアに書き込まれた状態エントリの数</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 新しいトランザクションを開始します。
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 現在のトランザクションをコミットします。
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 現在のトランザクションをロールバックします。
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
