using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.SocialGraph;

/// <summary>
/// フォロー関係を表す集約ルート
/// 通信メッセージではなく、自システム内に存在する状態を表します。
/// </summary>
public sealed class FollowRelation : AggregateRoot<FollowId>
{
    private FollowRelation(
        FollowId id,
        ActorId followerId,
        ActorId followeeId,
        FollowStatus status,
        DateTimeOffset createdAt
    )
        : base(id)
    {
        if (followerId == followeeId)
        {
            throw new ArgumentException(
                "FollowRelationのフォロワーとフォロイーに同じActorIdは指定できません。"
            );
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "FollowRelationの状態が不正です。"
            );
        }

        if (createdAt == default)
        {
            throw new ArgumentOutOfRangeException(
                nameof(createdAt),
                createdAt,
                "FollowRelation作成日時を指定してください。"
            );
        }

        FollowerId = followerId;
        FolloweeId = followeeId;
        Status = status;
        CreatedAt = createdAt;
    }

    /// <summary>フォローする側のActor</summary>
    public ActorId FollowerId { get; }

    /// <summary>フォローされる側のActor</summary>
    public ActorId FolloweeId { get; }

    public FollowStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// フォローリクエストを作成します。
    /// 承認制アカウントへのフォローは Pending、通常は Accepted になります。
    /// </summary>
    public static Result<FollowRelation> Follow(
        FollowId id,
        ActorId followerId,
        ActorId followeeId,
        bool requiresApproval,
        DateTimeOffset createdAt
    )
    {
        if (followerId == followeeId)
        {
            return Result<FollowRelation>.Failure(SocialGraphErrors.SelfFollowNotAllowed);
        }

        var status = requiresApproval ? FollowStatus.Pending : FollowStatus.Accepted;
        var relation = new FollowRelation(id, followerId, followeeId, status, createdAt);

        relation.AddDomainEvent(
            new FollowRequested(relation.Id, followerId, followeeId, status, createdAt)
        );

        return Result<FollowRelation>.Success(relation);
    }

    /// <summary>
    /// 永続化層からの復元用ファクトリメソッド。ドメインイベントを発行しません。
    /// </summary>
    public static FollowRelation Reconstitute(
        FollowId id,
        ActorId followerId,
        ActorId followeeId,
        FollowStatus status,
        DateTimeOffset createdAt
    )
    {
        return new FollowRelation(id, followerId, followeeId, status, createdAt);
    }

    /// <summary>
    /// フォローリクエストを承認します（フォロイー側から）。
    /// </summary>
    public Result Accept(DateTimeOffset occurredAt)
    {
        if (Status == FollowStatus.Accepted)
        {
            // 冪等性: 既に承認済みなら成功扱い
            return Result.Success();
        }

        if (Status is not (FollowStatus.Pending))
        {
            return Result.Failure(SocialGraphErrors.FollowTransitionInvalid);
        }

        Status = FollowStatus.Accepted;
        AddDomainEvent(new FollowAccepted(Id, FollowerId, FolloweeId, occurredAt));
        return Result.Success();
    }

    /// <summary>
    /// フォローリクエストを拒否します（フォロイー側から）。
    /// </summary>
    public Result Reject(DateTimeOffset occurredAt)
    {
        if (Status == FollowStatus.Rejected)
        {
            // 冪等性: 既に拒否済みなら成功扱い
            return Result.Success();
        }

        if (Status is not (FollowStatus.Pending or FollowStatus.Accepted))
        {
            return Result.Failure(SocialGraphErrors.FollowTransitionInvalid);
        }

        Status = FollowStatus.Rejected;
        AddDomainEvent(new FollowRejected(Id, FollowerId, FolloweeId, occurredAt));
        return Result.Success();
    }

    /// <summary>
    /// フォローを解除します（フォロワー側からの Unfollow）。
    /// </summary>
    public Result Unfollow(DateTimeOffset occurredAt)
    {
        if (Status == FollowStatus.Removed)
        {
            // 冪等性
            return Result.Success();
        }

        if (Status is FollowStatus.Blocked)
        {
            return Result.Failure(SocialGraphErrors.FollowTransitionInvalid);
        }

        Status = FollowStatus.Removed;
        AddDomainEvent(new FollowRemoved(Id, FollowerId, FolloweeId, occurredAt));
        return Result.Success();
    }

    /// <summary>
    /// フォロワーを削除します（フォロイー側からの RemoveFollower）。
    /// </summary>
    public Result RemoveFollower(DateTimeOffset occurredAt)
    {
        if (Status == FollowStatus.Removed)
        {
            // 冪等性
            return Result.Success();
        }

        if (Status is FollowStatus.Blocked)
        {
            return Result.Failure(SocialGraphErrors.FollowTransitionInvalid);
        }

        Status = FollowStatus.Removed;
        AddDomainEvent(new FollowerRemoved(Id, FollowerId, FolloweeId, occurredAt));
        return Result.Success();
    }

    /// <summary>
    /// ブロックによりフォロー関係を停止します（Block集約との整合用）。
    /// </summary>
    public Result Block(DateTimeOffset occurredAt)
    {
        if (Status == FollowStatus.Blocked)
        {
            // 冪等性
            return Result.Success();
        }

        if (Status == FollowStatus.Removed)
        {
            return Result.Failure(SocialGraphErrors.FollowTransitionInvalid);
        }

        Status = FollowStatus.Blocked;
        AddDomainEvent(new FollowBlocked(Id, FollowerId, FolloweeId, occurredAt));
        return Result.Success();
    }
}
