namespace Fediva.Domain.Identity.Actors;

/// <summary>
/// Actorのライフサイクル上の状態を表します。
/// </summary>
public enum ActorStatus
{
    /// <summary>通常活動中</summary>
    Active,

    /// <summary>無効化・一時停止中</summary>
    Suspended,

    /// <summary>別のActorへ移行済み(終端状態)</summary>
    Migrated,

    /// <summary>削除済み(終端状態)</summary>
    Deleted,
}
