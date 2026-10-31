using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.Content;

/// <summary>
/// Unified Reaction集約ルート
/// ActivityPubのLikeとAT Protocolのapp.bsky.feed.likeから意味を統合したモデルです。
/// </summary>
public sealed class Reaction : AggregateRoot<ReactionId>
{
#pragma warning disable CS8618
    // EF Core用プライベートパラメータなしコンストラクタ
    private Reaction()
        : base(default) { }
#pragma warning restore CS8618

    private Reaction(
        ReactionId id,
        ActorId actorId,
        ContentRef target,
        ReactionKind kind,
        ReactionStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? removedAt
    )
        : base(id)
    {
        if (createdAt == default)
        {
            throw new ArgumentOutOfRangeException(
                nameof(createdAt),
                createdAt,
                "Reaction作成日時を指定してください。"
            );
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Reactionの種別が不正です。");
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Reactionの状態が不正です。"
            );
        }

        ActorId = actorId;
        Target = target;
        Kind = kind;
        Status = status;
        CreatedAt = createdAt;
        RemovedAt = removedAt;
    }

    public ActorId ActorId { get; private set; }

    public ContentRef Target { get; private set; }

    public ReactionKind Kind { get; private set; }

    public ReactionStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? RemovedAt { get; private set; }

    /// <summary>
    /// 新規Reactionを作成し、ReactionAddedドメインイベントを発行します。
    /// </summary>
    public static Result<Reaction> Create(
        ReactionId id,
        ActorId actorId,
        ContentRef target,
        ReactionKind kind,
        DateTimeOffset createdAt
    )
    {
        var reaction = new Reaction(
            id,
            actorId,
            target,
            kind,
            ReactionStatus.Active,
            createdAt,
            removedAt: null
        );

        reaction.AddDomainEvent(
            new ReactionAdded(
                reaction.Id,
                reaction.ActorId,
                reaction.Target,
                reaction.Kind,
                reaction.CreatedAt
            )
        );

        return Result<Reaction>.Success(reaction);
    }

    /// <summary>
    /// 永続化層からの復元用ファクトリメソッド。ドメインイベントを発行しません。
    /// </summary>
    public static Reaction Reconstitute(
        ReactionId id,
        ActorId actorId,
        ContentRef target,
        ReactionKind kind,
        ReactionStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? removedAt
    )
    {
        return new Reaction(id, actorId, target, kind, status, createdAt, removedAt);
    }

    /// <summary>
    /// 取り消されたReactionを再度有効化します。すでに有効な場合は冪等に成功を返します。
    /// </summary>
    public Result Reactivate(ActorId requestorId, DateTimeOffset reactivatedAt)
    {
        if (requestorId != ActorId)
        {
            return Result.Failure(ContentErrors.ReactionActorMismatch);
        }

        if (Status == ReactionStatus.Active)
        {
            // 冪等性: 既に有効な場合はイベントを重複発行しない
            return Result.Success();
        }

        Status = ReactionStatus.Active;
        RemovedAt = null;

        AddDomainEvent(new ReactionAdded(Id, ActorId, Target, Kind, reactivatedAt));
        return Result.Success();
    }

    /// <summary>
    /// Reactionを取り消し（削除）し、ReactionRemovedドメインイベントを発行します。
    /// </summary>
    /// <param name="requestorId">取り消しを要求するActorのId</param>
    /// <param name="removedAt">取り消し日時</param>
    public Result Remove(ActorId requestorId, DateTimeOffset removedAt)
    {
        if (requestorId != ActorId)
        {
            return Result.Failure(ContentErrors.ReactionActorMismatch);
        }

        if (Status == ReactionStatus.Removed)
        {
            // 冪等性: 既に取り消し済みなら成功扱いでイベントを重複発行しない
            return Result.Success();
        }

        Status = ReactionStatus.Removed;
        RemovedAt = removedAt;

        AddDomainEvent(new ReactionRemoved(Id, ActorId, Target, Kind, removedAt));
        return Result.Success();
    }

    /// <summary>
    /// リモートからインポートされたReactionを作成します(ReactionAddedイベントを発行します)。
    /// </summary>
    public static Result<Reaction> Import(
        ReactionId id,
        ActorId actorId,
        ContentRef target,
        ReactionKind kind,
        DateTimeOffset createdAt
    ) => Create(id, actorId, target, kind, createdAt);

    /// <summary>
    /// リモートからの取り消し通知を適用します(ImportUndoHandler向け)。
    /// 著者チェックを省略する内部操作です。
    /// </summary>
    public Result ImportRemoval(DateTimeOffset removedAt)
    {
        if (Status == ReactionStatus.Removed)
        {
            return Result.Success();
        }

        Status = ReactionStatus.Removed;
        RemovedAt = removedAt;

        AddDomainEvent(new ReactionRemoved(Id, ActorId, Target, Kind, removedAt));
        return Result.Success();
    }
}
