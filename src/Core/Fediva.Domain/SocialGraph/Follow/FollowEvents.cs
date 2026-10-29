using Fediva.Domain.Common;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.SocialGraph;

/// <summary>フォローリクエストが送信されたことを表すドメインイベント</summary>
public sealed record FollowRequested(
    FollowId FollowId,
    ActorId FollowerId,
    ActorId FolloweeId,
    FollowStatus InitialStatus,
    DateTimeOffset OccurredAt
) : DomainEvent;

/// <summary>フォローリクエストが承認されたことを表すドメインイベント</summary>
public sealed record FollowAccepted(
    FollowId FollowId,
    ActorId FollowerId,
    ActorId FolloweeId,
    DateTimeOffset OccurredAt
) : DomainEvent;

/// <summary>フォローリクエストが拒否されたことを表すドメインイベント</summary>
public sealed record FollowRejected(
    FollowId FollowId,
    ActorId FollowerId,
    ActorId FolloweeId,
    DateTimeOffset OccurredAt
) : DomainEvent;

/// <summary>フォローが解除されたことを表すドメインイベント(フォロー側からの解除)</summary>
public sealed record FollowRemoved(
    FollowId FollowId,
    ActorId FollowerId,
    ActorId FolloweeId,
    DateTimeOffset OccurredAt
) : DomainEvent;

/// <summary>フォロワーが削除されたことを表すドメインイベント(フォロイー側からの削除)</summary>
public sealed record FollowerRemoved(
    FollowId FollowId,
    ActorId FollowerId,
    ActorId FolloweeId,
    DateTimeOffset OccurredAt
) : DomainEvent;

/// <summary>ブロックによりフォロー関係が停止されたことを表すドメインイベント</summary>
public sealed record FollowBlocked(
    FollowId FollowId,
    ActorId FollowerId,
    ActorId FolloweeId,
    DateTimeOffset OccurredAt
) : DomainEvent;
