namespace Fediva.Domain.Content;

/// <summary>
/// Reactionのライフサイクル上の状態を表します。
/// </summary>
public enum ReactionStatus
{
    /// <summary>有効(リアクション中)</summary>
    Active,

    /// <summary>取り消し済み</summary>
    Removed,
}
