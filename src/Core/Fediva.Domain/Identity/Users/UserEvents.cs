using Fediva.Domain.Common;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.Identity.Users;

public sealed record UserCreated(UserId UserId, DisplayName DisplayName, EmailAddress EmailAddress)
    : DomainEvent;

public sealed record DisplayNameChanged(UserId UserId, DisplayName Previous, DisplayName Current)
    : DomainEvent;

public sealed record EmailAddressChanged(UserId UserId, EmailAddress Previous, EmailAddress Current)
    : DomainEvent;

public sealed record UserActorAdded(UserId UserId, ActorId ActorId) : DomainEvent;

public sealed record UserActorRemoved(UserId UserId, ActorId ActorId) : DomainEvent;

public sealed record UserStatusChanged(UserId UserId, UserStatus Previous, UserStatus Current)
    : DomainEvent;
