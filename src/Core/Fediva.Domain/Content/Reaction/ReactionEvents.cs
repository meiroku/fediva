using Fediva.Domain.Common;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.Content;

/// <summary>新しいReactionが追加されたことを表すドメインイベント</summary>
public sealed record ReactionAdded(
    ReactionId ReactionId,
    ActorId ActorId,
    ContentRef Target,
    ReactionKind Kind,
    DateTimeOffset CreatedAt
) : DomainEvent;

/// <summary>Reactionが取り消しされたことを表すドメインイベント</summary>
public sealed record ReactionRemoved(
    ReactionId ReactionId,
    ActorId ActorId,
    ContentRef Target,
    ReactionKind Kind,
    DateTimeOffset RemovedAt
) : DomainEvent;
