namespace Fediva.Application;

/// <summary>
/// 戻り値を持たないコマンドを表すマーカーインターフェース。
/// </summary>
public interface ICommand;

/// <summary>
/// 戻り値を持つコマンドを表すマーカーインターフェース。
/// </summary>
/// <typeparam name="TResult">コマンド実行結果の型</typeparam>
public interface ICommand<TResult>;
