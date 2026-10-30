using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.Content;

/// <summary>
/// Unified Reply集約ルート
/// ActivityPubのNote(inReplyTo付き)とAT Protocolのapp.bsky.feed.post(reply付き)から意味を統合したモデルです。
/// </summary>
public sealed class Reply : AggregateRoot<ReplyId>
{
#pragma warning disable CS8618
    // EF Core用プライベートパラメータなしコンストラクタ
    private Reply()
        : base(default) { }
#pragma warning restore CS8618

    private Reply(
        ReplyId id,
        ActorId authorId,
        ReplyTarget target,
        PostContent content,
        Visibility visibility,
        ReplyStatus status,
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
                "Reply作成日時を指定してください。"
            );
        }

        if (!Enum.IsDefined(visibility))
        {
            throw new ArgumentOutOfRangeException(
                nameof(visibility),
                visibility,
                "Replyの公開範囲が不正です。"
            );
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Replyの状態が不正です。"
            );
        }

        AuthorId = authorId;
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Visibility = visibility;
        Status = status;
        CreatedAt = createdAt;
        EditedAt = editedAt;
    }

    public ActorId AuthorId { get; private set; }

    public ReplyTarget Target { get; private set; }

    public PostContent Content { get; private set; }

    public Visibility Visibility { get; private set; }

    public ReplyStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? EditedAt { get; private set; }

    /// <summary>
    /// 新規Replyを作成し、ReplyCreatedドメインイベントを発行します。
    /// </summary>
    public static Result<Reply> Create(
        ReplyId id,
        ActorId authorId,
        ReplyTarget target,
        PostContent content,
        Visibility visibility,
        DateTimeOffset createdAt
    )
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(content);

        var reply = new Reply(
            id,
            authorId,
            target,
            content,
            visibility,
            ReplyStatus.Active,
            createdAt,
            editedAt: null
        );

        reply.AddDomainEvent(
            new ReplyCreated(
                reply.Id,
                reply.AuthorId,
                reply.Target,
                reply.Content.Text,
                reply.Visibility,
                reply.CreatedAt
            )
        );

        return Result<Reply>.Success(reply);
    }

    /// <summary>
    /// 永続化層からの復元用ファクトリメソッド。ドメインイベントを発行しません。
    /// </summary>
    public static Reply Reconstitute(
        ReplyId id,
        ActorId authorId,
        ReplyTarget target,
        PostContent content,
        Visibility visibility,
        ReplyStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? editedAt
    )
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(content);
        return new Reply(id, authorId, target, content, visibility, status, createdAt, editedAt);
    }

    /// <summary>
    /// Reply本文を編集し、ReplyEditedドメインイベントを発行します。
    /// </summary>
    /// <param name="editorId">編集を行うActorのId（著者と一致する必要があります）</param>
    /// <param name="newContent">新しい本文</param>
    /// <param name="editedAt">編集日時</param>
    public Result Edit(ActorId editorId, PostContent newContent, DateTimeOffset editedAt)
    {
        ArgumentNullException.ThrowIfNull(newContent);

        if (editorId != AuthorId)
        {
            return Result.Failure(ContentErrors.ReplyEditorMismatch);
        }

        if (Status == ReplyStatus.Deleted)
        {
            return Result.Failure(ContentErrors.ReplyNotEditable);
        }

        var previousText = Content.Text;
        var isContentChanged = Content != newContent;

        if (!isContentChanged)
        {
            return Result.Success();
        }

        Content = newContent;
        EditedAt = editedAt;

        AddDomainEvent(new ReplyEdited(Id, AuthorId, previousText, Content.Text, editedAt));
        return Result.Success();
    }

    /// <summary>
    /// Replyを論理削除し、ReplyDeletedドメインイベントを発行します。
    /// </summary>
    /// <param name="requestorId">削除を要求するActorのId</param>
    /// <param name="deletedAt">削除日時</param>
    public Result Delete(ActorId requestorId, DateTimeOffset deletedAt)
    {
        if (requestorId != AuthorId)
        {
            return Result.Failure(ContentErrors.ReplyEditorMismatch);
        }

        if (Status == ReplyStatus.Deleted)
        {
            // 冪等性: 既に削除済みなら成功扱いでイベントを重複発行しない
            return Result.Success();
        }

        Status = ReplyStatus.Deleted;
        AddDomainEvent(new ReplyDeleted(Id, AuthorId, deletedAt));
        return Result.Success();
    }

    /// <summary>
    /// リモートからインポートされたReplyを作成します(ReplyCreatedイベントを発行します)。
    /// </summary>
    public static Result<Reply> Import(
        ReplyId id,
        ActorId authorId,
        ReplyTarget target,
        PostContent content,
        Visibility visibility,
        DateTimeOffset createdAt
    ) => Create(id, authorId, target, content, visibility, createdAt);

    /// <summary>
    /// リモートからの削除通知を適用します(ImportDeletionHandler向け)。
    /// 著者チェックを省略する内部操作です。
    /// </summary>
    public Result ImportDeletion(DateTimeOffset deletedAt)
    {
        if (Status == ReplyStatus.Deleted)
        {
            return Result.Success();
        }

        Status = ReplyStatus.Deleted;
        AddDomainEvent(new ReplyDeleted(Id, AuthorId, deletedAt));
        return Result.Success();
    }
}
