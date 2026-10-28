using Fediva.Domain.Common;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.Content;

/// <summary>新しいPostが作成されたことを表すドメインイベント</summary>
public sealed record PostCreated(
    PostId PostId,
    ActorId AuthorId,
    string Text,
    Visibility Visibility,
    PostId? QuotedPostId,
    DateTimeOffset CreatedAt
) : DomainEvent;

/// <summary>Postが編集されたことを表すドメインイベント</summary>
public sealed record PostEdited(
    PostId PostId,
    ActorId AuthorId,
    string PreviousText,
    string CurrentText,
    DateTimeOffset EditedAt
) : DomainEvent;

/// <summary>Postが削除されたことを表すドメインイベント</summary>
public sealed record PostDeleted(PostId PostId, ActorId AuthorId, DateTimeOffset DeletedAt)
    : DomainEvent;
