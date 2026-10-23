using Fediva.Domain.Common;

namespace Fediva.Application;

/// <summary>
/// クエリを処理するハンドラーのインターフェース。
/// </summary>
/// <typeparam name="TQuery">処理対象のクエリの型</typeparam>
/// <typeparam name="TResult">処理結果の値の型</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// クエリを非同期で処理します。
    /// </summary>
    /// <param name="query">実行するクエリ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>処理結果を表す <see cref="Result{TResult}"/></returns>
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
