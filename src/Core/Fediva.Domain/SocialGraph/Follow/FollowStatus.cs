namespace Fediva.Domain.SocialGraph;

/// <summary>
/// FollowRelationの状態を表します。
/// </summary>
public enum FollowStatus
{
    /// <summary>承認待ち</summary>
    Pending,

    /// <summary>フォロー承認済み</summary>
    Accepted,

    /// <summary>フォロー拒否済み</summary>
    Rejected,

    /// <summary>ブロックによる関係停止</summary>
    Blocked,

    /// <summary>フォロー解除またはフォロワー削除による関係終了</summary>
    Removed,
}
