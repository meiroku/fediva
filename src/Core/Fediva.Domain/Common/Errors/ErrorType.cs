namespace Fediva.Domain.Common.Errors;

public enum ErrorType
{
    /// <summary>
    /// エラーがない場合
    /// </summary>
    None = 0,

    /// <summary>
    /// 予期していない問題が発生した場合
    /// </summary>
    Failure,

    /// <summary>
    /// 値または操作が、ドメイン検証で違反した場合
    /// </summary>
    Validation,

    /// <summary>
    /// リクエストされたリソースが存在しない場合
    /// </summary>
    NotFound,

    /// <summary>
    /// 現在のドメインの状態に操作が衝突した場合
    /// </summary>
    Conflict,

    /// <summary>
    /// 操作の認証が必要な場合
    /// </summary>
    Unauthorized,

    /// <summary>
    /// 操作が許可されていない場合
    /// </summary>
    Forbidden,
}
