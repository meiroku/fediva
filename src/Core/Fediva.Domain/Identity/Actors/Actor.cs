using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;

namespace Fediva.Domain.Identity.Actors;

/// <summary>
/// SNS上で活動する人格(Actor)を表す集約ルート
/// </summary>
public sealed class Actor : AggregateRoot<ActorId>
{
    private Actor(
        ActorId id,
        ActorKind kind,
        ActorOrigin origin,
        ActorStatus status,
        ActorProfile profile,
        DateTimeOffset createdAt,
        ActorId? movedToActorId = null
    )
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Actorの種別が不正です。");
        }

        if (!Enum.IsDefined(origin))
        {
            throw new ArgumentOutOfRangeException(
                nameof(origin),
                origin,
                "Actorの出自が不正です。"
            );
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Actorの状態が不正です。"
            );
        }

        if (createdAt == default)
        {
            throw new ArgumentOutOfRangeException(
                nameof(createdAt),
                createdAt,
                "Actor作成日時を指定してください。"
            );
        }

        Kind = kind;
        Origin = origin;
        Status = status;
        Profile = profile;
        CreatedAt = createdAt;
        MovedToActorId = movedToActorId;
    }

    /// <summary>
    /// 新規Actorを作成し、ドメインイベント <see cref="ActorCreated"/> を発行します。
    /// </summary>
    public static Actor Create(
        ActorId id,
        ActorKind kind,
        ActorOrigin origin,
        ActorProfile profile,
        DateTimeOffset createdAt
    )
    {
        var actor = new Actor(id, kind, origin, ActorStatus.Active, profile, createdAt);
        actor.AddDomainEvent(new ActorCreated(actor.Id, actor.Kind, actor.Origin, actor.Profile));
        return actor;
    }

    /// <summary>
    /// 永続化層からの復元用ファクトリメソッド。ドメインイベントを発行しません。
    /// </summary>
    public static Actor Reconstitute(
        ActorId id,
        ActorKind kind,
        ActorOrigin origin,
        ActorStatus status,
        ActorProfile profile,
        DateTimeOffset createdAt,
        ActorId? movedToActorId = null
    )
    {
        return new Actor(id, kind, origin, status, profile, createdAt, movedToActorId);
    }

    public ActorKind Kind { get; }

    public ActorOrigin Origin { get; }

    public ActorStatus Status { get; private set; }

    public ActorProfile Profile { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public ActorId? MovedToActorId { get; private set; }

    /// <summary>
    /// プロフィール情報を更新します。
    /// </summary>
    public Result UpdateProfile(ActorProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (Status is ActorStatus.Deleted or ActorStatus.Migrated)
        {
            return Result.Failure(IdentityErrors.ActorStatusTransitionInvalid);
        }

        if (Profile == profile)
        {
            return Result.Success();
        }

        var previous = Profile;
        Profile = profile;
        AddDomainEvent(new ActorProfileChanged(Id, previous, Profile));
        return Result.Success();
    }

    /// <summary>
    /// Actorを一時停止・無効化します。
    /// </summary>
    public Result Suspend() => TransitionTo(ActorStatus.Suspended);

    /// <summary>
    /// 一時停止中のActorを再有効化します。
    /// </summary>
    public Result Activate() => TransitionTo(ActorStatus.Active);

    /// <summary>
    /// Actorを削除します。
    /// </summary>
    public Result Delete() => TransitionTo(ActorStatus.Deleted);

    /// <summary>
    /// Actorを別のActorへ移行します。
    /// </summary>
    public Result Migrate(ActorId destinationActorId)
    {
        if (destinationActorId == Id)
        {
            return Result.Failure(IdentityErrors.ActorMigrationDestinationInvalid);
        }

        if (Status != ActorStatus.Active)
        {
            return Result.Failure(IdentityErrors.ActorStatusTransitionInvalid);
        }

        var previous = Status;
        Status = ActorStatus.Migrated;
        MovedToActorId = destinationActorId;
        AddDomainEvent(new ActorStatusChanged(Id, previous, Status));
        AddDomainEvent(new ActorMigrated(Id, destinationActorId));
        return Result.Success();
    }

    private Result TransitionTo(ActorStatus status)
    {
        if (Status == status)
        {
            return Result.Success();
        }

        if (Status is ActorStatus.Deleted or ActorStatus.Migrated)
        {
            return Result.Failure(IdentityErrors.ActorStatusTransitionInvalid);
        }

        var previous = Status;
        Status = status;
        AddDomainEvent(new ActorStatusChanged(Id, previous, Status));
        return Result.Success();
    }
}
