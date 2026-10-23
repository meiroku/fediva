using Fediva.Domain.Common.Errors;

namespace Fediva.Application;

/// <summary>
/// アプリケーション層で共通して使用されるエラー定義
/// </summary>
public static class ApplicationErrors
{
    /// <summary>
    /// 認証済みユーザーが必要であることを示すエラー
    /// </summary>
    public static Error CurrentUserRequired { get; } =
        new(
            "application.current_user.required",
            "An authenticated user is required.",
            ErrorType.Unauthorized
        );
}
