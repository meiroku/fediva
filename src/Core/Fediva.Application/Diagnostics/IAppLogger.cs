namespace Fediva.Application.Diagnostics;

/// <summary>
/// アプリケーション層向けの構造化ログ出力ポート
/// </summary>
/// <typeparam name="TCategory">ログカテゴリの型</typeparam>
public interface IAppLogger<TCategory>
{
    /// <summary>
    /// 情報レベルのログを出力します。
    /// </summary>
    void LogInformation(string message, params object?[] args);

    /// <summary>
    /// 警告レベルのログを出力します。
    /// </summary>
    void LogWarning(string message, params object?[] args);

    /// <summary>
    /// エラーレベルのログを出力します。
    /// </summary>
    void LogError(string message, params object?[] args);

    /// <summary>
    /// 例外を伴うエラーレベルのログを出力します。
    /// </summary>
    void LogError(Exception exception, string message, params object?[] args);
}
