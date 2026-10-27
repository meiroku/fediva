using Fediva.Domain.Common;

namespace Fediva.Domain.Identity.Actors;

/// <summary>
/// 新しいActorが作成されたことを表すドメインイベント
/// </summary>
public sealed record ActorCreated(
    ActorId ActorId,
    ActorKind Kind,
    ActorOrigin Origin,
    ActorProfile Profile
) : DomainEvent;

/// <summary>
/// Actorのプロフィールが変更されたことを表すドメインイベント
/// </summary>
public sealed record ActorProfileChanged(
    ActorId ActorId,
    ActorProfile Previous,
    ActorProfile Current
) : DomainEvent;

/// <summary>
/// Actorのステータスが変更されたことを表すドメインイベント
/// </summary>
public sealed record ActorStatusChanged(ActorId ActorId, ActorStatus Previous, ActorStatus Current)
    : DomainEvent;

/// <summary>
/// Actorが別のActorへ移行したことを表すドメインイベント
/// </summary>
public sealed record ActorMigrated(ActorId ActorId, ActorId DestinationActorId) : DomainEvent;
