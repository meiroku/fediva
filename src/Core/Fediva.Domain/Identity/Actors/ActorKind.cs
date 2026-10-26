namespace Fediva.Domain.Identity.Actors;

/// <summary>
/// Actorの種別を表します。
/// </summary>
public enum ActorKind
{
    /// <summary>個人</summary>
    Person,

    /// <summary>ボット・自動応答アカウント</summary>
    Bot,

    /// <summary>組織・企業・団体</summary>
    Organization,

    /// <summary>サービス・システムアカウント</summary>
    Service,

    /// <summary>グループ・コミュニティ</summary>
    Group,
}
