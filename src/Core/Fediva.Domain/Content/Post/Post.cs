using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.Content;

/// <summary>
/// Unified Post集約ルート
/// ActivityPub の Create+Note と app.bsky.feed.post の両方から意味だけを統合したモデルです。
/// ローカル投稿もリモート投稿（ImportPostHandler 経由）も同じ Post で表します。
/// ローカルかリモートかは Author の ActorOrigin で判断します。
/// </summary>
public sealed class Post : AggregateRoot<PostId>
{
    private Post(
        PostId id,
        ActorId authorId,
        PostContent content,
        Visibility visibility,
        QuoteReference? quoteReference,
        PostStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? editedAt
    )
        : base(id)
    {
        if (createdAt == default)
        {
            throw new ArgumentOutOfRangeException(
                nameof(createdAt),
                createdAt,
                "Post作成日時を指定してください。"
            );
        }

        if (!Enum.IsDefined(visibility))
        {
            throw new ArgumentOutOfRangeException(
                nameof(visibility),
                visibility,
                "Postの公開範囲が不正です。"
            );
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Postの状態が不正です。");
        }

        AuthorId = authorId;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Visibility = visibility;
        QuoteReference = quoteReference;
        Status = status;
        CreatedAt = createdAt;
        EditedAt = editedAt;
    }

    public ActorId AuthorId { get; }

    public PostContent Content { get; private set; }

    public Visibility Visibility { get; private set; }

    public QuoteReference? QuoteReference { get; }

    public PostStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? EditedAt { get; private set; }

    /// <summary>
    /// 新規Postを作成し、PostCreated ドメインイベントを発行します。
    /// </summary>
    public static Result<Post> Create(
        PostId id,
        ActorId authorId,
        PostContent content,
        Visibility visibility,
        DateTimeOffset createdAt,
        QuoteReference? quoteReference = null
    )
    {
        ArgumentNullException.ThrowIfNull(content);

        var post = new Post(
            id,
            authorId,
            content,
            visibility,
            quoteReference,
            PostStatus.Active,
            createdAt,
            editedAt: null
        );

        post.AddDomainEvent(
            new PostCreated(
                post.Id,
                post.AuthorId,
                post.Content.Text,
                post.Visibility,
                post.QuoteReference?.QuotedPostId,
                post.CreatedAt
            )
        );

        return Result<Post>.Success(post);
    }

    /// <summary>
    /// 永続化層からの復元用ファクトリメソッド。ドメインイベントを発行しません。
    /// </summary>
    public static Post Reconstitute(
        PostId id,
        ActorId authorId,
        PostContent content,
        Visibility visibility,
        QuoteReference? quoteReference,
        PostStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? editedAt
    )
    {
        ArgumentNullException.ThrowIfNull(content);
        return new Post(
            id,
            authorId,
            content,
            visibility,
            quoteReference,
            status,
            createdAt,
            editedAt
        );
    }

    /// <summary>
    /// Post本文と公開範囲を編集し、PostEditedドメインイベントを発行します。
    /// </summary>
    /// <param name="editorId">編集を行うActorのId（著者と一致する必要があります）</param>
    /// <param name="newContent">新しい本文</param>
    /// <param name="newVisibility">新しい公開範囲</param>
    /// <param name="editedAt">編集日時</param>
    public Result Edit(
        ActorId editorId,
        PostContent newContent,
        Visibility newVisibility,
        DateTimeOffset editedAt
    )
    {
        ArgumentNullException.ThrowIfNull(newContent);

        if (editorId != AuthorId)
        {
            return Result.Failure(ContentErrors.PostEditorMismatch);
        }

        if (Status == PostStatus.Deleted)
        {
            return Result.Failure(ContentErrors.PostNotEditable);
        }

        if (!Enum.IsDefined(newVisibility))
        {
            return Result.Failure(ContentErrors.PostNotEditable);
        }

        var previousText = Content.Text;
        var isContentChanged = Content != newContent || Visibility != newVisibility;

        if (!isContentChanged)
        {
            // 冪等性: 同じ内容への編集はイベントを発行しない
            return Result.Success();
        }

        Content = newContent;
        Visibility = newVisibility;
        EditedAt = editedAt;

        AddDomainEvent(new PostEdited(Id, AuthorId, previousText, Content.Text, editedAt));
        return Result.Success();
    }

    /// <summary>
    /// Postを論理削除し、PostDeletedドメインイベントを発行します。
    /// </summary>
    /// <param name="requestorId">削除を要求するActorのId</param>
    /// <param name="deletedAt">削除日時</param>
    public Result Delete(ActorId requestorId, DateTimeOffset deletedAt)
    {
        if (requestorId != AuthorId)
        {
            return Result.Failure(ContentErrors.PostEditorMismatch);
        }

        if (Status == PostStatus.Deleted)
        {
            // 冪等性: 既に削除済みなら成功扱いでイベントを重複発行しない
            return Result.Success();
        }

        Status = PostStatus.Deleted;
        AddDomainEvent(new PostDeleted(Id, AuthorId, deletedAt));
        return Result.Success();
    }

    /// <summary>
    /// リモートからインポートされたPostを作成します(PostCreated イベントを発行します)。
    /// </summary>
    public static Result<Post> Import(
        PostId id,
        ActorId authorId,
        PostContent content,
        Visibility visibility,
        DateTimeOffset createdAt,
        QuoteReference? quoteReference = null
    ) => Create(id, authorId, content, visibility, createdAt, quoteReference);

    /// <summary>
    /// リモートからの削除通知を適用します(ImportDeletionHandler向け)。
    /// 著者チェックを省略する内部操作です。
    /// </summary>
    public Result ImportDeletion(DateTimeOffset deletedAt)
    {
        if (Status == PostStatus.Deleted)
        {
            return Result.Success();
        }

        Status = PostStatus.Deleted;
        AddDomainEvent(new PostDeleted(Id, AuthorId, deletedAt));
        return Result.Success();
    }
}
