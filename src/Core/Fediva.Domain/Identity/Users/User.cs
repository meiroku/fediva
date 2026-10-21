using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.Identity.Users;

public sealed class User : AggregateRoot<UserId>
{
    private readonly List<ActorId> _actorIds = [];
    private readonly IReadOnlyCollection<ActorId> _readOnlyActorIds;

    private User(
        UserId id,
        DisplayName displayName,
        EmailAddress emailAddress,
        DateTimeOffset createdAt
    )
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(displayName);
        ArgumentNullException.ThrowIfNull(emailAddress);

        if (createdAt == default)
        {
            throw new ArgumentOutOfRangeException(
                nameof(createdAt),
                createdAt,
                "ユーザー作成日時を指定してください。"
            );
        }

        DisplayName = displayName;
        EmailAddress = emailAddress;
        CreatedAt = createdAt;
        Status = UserStatus.Active;
        _readOnlyActorIds = _actorIds.AsReadOnly();
    }

    public static User Create(
        UserId id,
        DisplayName displayName,
        EmailAddress emailAddress,
        DateTimeOffset createdAt
    )
    {
        var user = new User(id, displayName, emailAddress, createdAt);
        user.AddDomainEvent(new UserCreated(user.Id, user.DisplayName, user.EmailAddress));
        return user;
    }

    public DisplayName DisplayName { get; private set; }

    public EmailAddress EmailAddress { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public UserStatus Status { get; private set; }

    /// <summary>
    /// このユーザーに紐づくActorのIDコレクション。
    /// Userはログイン主体として複数のActorを保持できる。
    /// </summary>
    public IReadOnlyCollection<ActorId> ActorIds => _readOnlyActorIds;

    public Result ChangeDisplayName(DisplayName displayName)
    {
        ArgumentNullException.ThrowIfNull(displayName);

        if (DisplayName == displayName)
        {
            return Result.Success();
        }

        var previous = DisplayName;
        DisplayName = displayName;
        AddDomainEvent(new DisplayNameChanged(Id, previous, DisplayName));
        return Result.Success();
    }

    public Result ChangeEmailAddress(EmailAddress emailAddress)
    {
        ArgumentNullException.ThrowIfNull(emailAddress);

        if (EmailAddress == emailAddress)
        {
            return Result.Success();
        }

        var previous = EmailAddress;
        EmailAddress = emailAddress;
        AddDomainEvent(new EmailAddressChanged(Id, previous, EmailAddress));
        return Result.Success();
    }

    public Result AddActor(ActorId actorId)
    {
        if (_actorIds.Contains(actorId))
        {
            return Result.Success();
        }

        _actorIds.Add(actorId);
        AddDomainEvent(new UserActorAdded(Id, actorId));
        return Result.Success();
    }

    public Result RemoveActor(ActorId actorId)
    {
        if (!_actorIds.Remove(actorId))
        {
            return Result.Success();
        }

        AddDomainEvent(new UserActorRemoved(Id, actorId));
        return Result.Success();
    }

    public Result Suspend() => TransitionTo(UserStatus.Suspended);

    public Result Activate() => TransitionTo(UserStatus.Active);

    public Result Withdraw() => TransitionTo(UserStatus.Withdrawn);

    public Result Delete() => TransitionTo(UserStatus.Deleted);

    private Result TransitionTo(UserStatus status)
    {
        if (Status == status)
        {
            return Result.Success();
        }

        if (Status is UserStatus.Withdrawn or UserStatus.Deleted)
        {
            return Result.Failure(IdentityErrors.UserStatusTransitionInvalid);
        }

        var previous = Status;
        Status = status;
        AddDomainEvent(new UserStatusChanged(Id, previous, Status));
        return Result.Success();
    }
}
