using Fediva.Domain.Common;

namespace Fediva.Application;

/// <summary>
/// 戻り値を持たないコマンドを処理するハンドラーのインターフェース。
/// </summary>
/// <typeparam name="TCommand">処理対象のコマンドの型</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// コマンドを非同期で処理します。
    /// </summary>
    /// <param name="command">実行するコマンド。</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>処理結果を表す <see cref="Result"/>。</returns>
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// 戻り値を持つコマンドを処理するハンドラーのインターフェース。
/// </summary>
/// <typeparam name="TCommand">処理対象のコマンドの型</typeparam>
/// <typeparam name="TResult">処理結果の値の型</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// コマンドを非同期で処理します。
    /// </summary>
    /// <param name="command">実行するコマンド</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>処理結果を表す <see cref="Result{TResult}"/></returns>
    Task<Result<TResult>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    );
}
