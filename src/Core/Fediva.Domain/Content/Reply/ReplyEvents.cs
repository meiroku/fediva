using Fediva.Domain.Common;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.Content;

/// <summary>新しいReplyが作成されたことを表すドメインイベント</summary>
public sealed record ReplyCreated(
    ReplyId ReplyId,
    ActorId AuthorId,
    ReplyTarget Target,
    string Text,
    Visibility Visibility,
    DateTimeOffset CreatedAt
) : DomainEvent;

/// <summary>Replyが編集されたことを表すドメインイベント</summary>
public sealed record ReplyEdited(
    ReplyId ReplyId,
    ActorId AuthorId,
    string PreviousText,
    string CurrentText,
    DateTimeOffset EditedAt
) : DomainEvent;

/// <summary>Replyが削除されたことを表すドメインイベント</summary>
public sealed record ReplyDeleted(ReplyId ReplyId, ActorId AuthorId, DateTimeOffset DeletedAt)
    : DomainEvent;
