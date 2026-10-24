using Fediva.Domain.Identity.Users;

namespace Fediva.Application.Identity.Queries;

/// <summary>
/// ユーザー情報の読み取り専用データ転送オブジェクト/ビューモデル
/// </summary>
/// <param name="Id">ユーザーID</param>
/// <param name="DisplayName">表示名</param>
/// <param name="EmailAddress">メールアドレス</param>
/// <param name="Status">ユーザー状態</param>
/// <param name="CreatedAt">作成日時</param>
public sealed record GetUserDto(
    UserId Id,
    DisplayName DisplayName,
    EmailAddress EmailAddress,
    UserStatus Status,
    DateTimeOffset CreatedAt
);

/// <summary>
/// ユーザー情報のビューモデル（<see cref="GetUserDto"/> のエイリアス）
/// </summary>
public sealed record UserView(
    UserId Id,
    DisplayName DisplayName,
    EmailAddress EmailAddress,
    UserStatus Status,
    DateTimeOffset CreatedAt
);
