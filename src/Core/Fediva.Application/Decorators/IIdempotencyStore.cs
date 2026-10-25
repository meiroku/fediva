namespace Fediva.Application.Decorators;

/// <summary>
/// 冪等性キーの状態管理を行うストアポート
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// 指定されたキーがすでに処理済みであるかを確認します。
    /// </summary>
    /// <param name="key">冪等性キー</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>処理済みの場合は <c>true</c>、未処理の場合は <c>false</c>。</returns>
    Task<bool> HasBeenProcessedAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定されたキーを処理済みとして記録します。
    /// </summary>
    /// <param name="key">冪等性キー</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    Task MarkAsProcessedAsync(string key, CancellationToken cancellationToken = default);
}
