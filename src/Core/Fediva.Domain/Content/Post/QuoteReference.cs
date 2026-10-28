namespace Fediva.Domain.Content;

/// <summary>
/// 引用対象の内部Postを参照するValue Object
/// AT Protocolは対応。ActivityPubは拡張依存のため、Domain層では内部PostIdで参照します。
/// </summary>
public sealed record QuoteReference
{
    public QuoteReference(PostId quotedPostId)
    {
        QuotedPostId = quotedPostId;
    }

    public PostId QuotedPostId { get; }
}
