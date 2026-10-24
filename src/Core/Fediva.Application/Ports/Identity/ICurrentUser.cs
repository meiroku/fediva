using Fediva.Domain.Identity.Actors;
using Fediva.Domain.Identity.Users;

namespace Fediva.Application.Ports.Identity;

/// <summary>
/// 現在のリクエストコンテキストにおける認証中ユーザー情報を表すポート
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// 現在認証されているユーザーのID。未認証の場合は <c>null</c>。
    /// </summary>
    UserId? Id { get; }

    /// <summary>
    /// 既存参照用の互換プロパティ。<see cref="Id"/> と等価です。
    /// </summary>
    UserId? UserId => Id;

    /// <summary>
    /// 現在アクティブなアクターID（Fediverse 活動主体）。未指定の場合は <c>null</c>。
    /// </summary>
    ActorId? ActiveActorId => null;

    /// <summary>
    /// ユーザーが認証済みであるかどうかを取得します。
    /// </summary>
    bool IsAuthenticated => Id.HasValue;
}
