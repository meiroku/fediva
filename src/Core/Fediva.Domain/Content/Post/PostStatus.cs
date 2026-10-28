namespace Fediva.Domain.Content;

/// <summary>
/// Postのライフサイクル上の状態を表します。
/// </summary>
public enum PostStatus
{
    /// <summary>通常公開中</summary>
    Active,

    /// <summary>論理削除済み(終端状態)</summary>
    Deleted,
}
