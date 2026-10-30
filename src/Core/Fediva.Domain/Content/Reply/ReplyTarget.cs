namespace Fediva.Domain.Content;

/// <summary>
/// 返信先(スレッドのルートと直接の親)を表すValue Object
/// ActivityPubは親(inReplyTo)のみ、AT Protocolはrootとparentの両方を保持します。
/// </summary>
public sealed record ReplyTarget
{
    public ReplyTarget(ContentRef root, ContentRef parent)
    {
        Root = root;
        Parent = parent;
    }

    public ContentRef Root { get; }

    public ContentRef Parent { get; }

    /// <summary>
    /// Postへの直接の返信（rootとparentが同一のPost）を生成します。
    /// </summary>
    public static ReplyTarget ToPost(PostId postId)
    {
        var postRef = ContentRef.FromPost(postId);
        return new ReplyTarget(postRef, postRef);
    }

    /// <summary>
    /// Replyへの返信(rootはスレッド根元のコンテンツ、parentは親Reply)を生成します。
    /// </summary>
    public static ReplyTarget ToReply(ContentRef root, ReplyId parentReplyId)
    {
        return new ReplyTarget(root, ContentRef.FromReply(parentReplyId));
    }
}
