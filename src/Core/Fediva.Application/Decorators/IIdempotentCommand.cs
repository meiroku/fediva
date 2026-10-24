namespace Fediva.Application.Decorators;

/// <summary>
/// 冪等性を保証するコマンドであることを示すマーカーインターフェース
/// </summary>
public interface IIdempotentCommand : ICommand
{
    /// <summary>
    /// コマンドの同一性を識別する冪等性キー
    /// </summary>
    string IdempotencyKey { get; }
}

/// <summary>
/// 戻り値を持つ、冪等性を保証するコマンドであることを示すマーカーインターフェース
/// </summary>
/// <typeparam name="TResult">コマンド実行結果の値の型</typeparam>
public interface IIdempotentCommand<TResult> : ICommand<TResult>
{
    /// <summary>
    /// コマンドの同一性を識別する冪等性キー
    /// </summary>
    string IdempotencyKey { get; }
}
