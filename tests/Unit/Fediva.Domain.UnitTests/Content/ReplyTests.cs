using Fediva.Domain.Common.Errors;
using Fediva.Domain.Content;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.UnitTests.Content;

public class ReplyTests
{
    private static readonly DateTimeOffset SampleCreatedAt = new(
        2025,
        1,
        2,
        3,
        4,
        5,
        TimeSpan.Zero
    );
    private static readonly DateTimeOffset SampleEditedAt = new(2025, 1, 2, 4, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset SampleDeletedAt = new(
        2025,
        1,
        2,
        5,
        0,
        0,
        TimeSpan.Zero
    );

    private static Reply CreateSampleReply(
        ActorId? authorId = null,
        Visibility visibility = Visibility.Public
    )
    {
        var id = ReplyId.New();
        var author = authorId ?? ActorId.New();
        var target = ReplyTarget.ToPost(PostId.New());
        var content = PostContent.Create("Sample reply text").Value;
        return Reply.Create(id, author, target, content, visibility, SampleCreatedAt).Value;
    }

    // --- Create ---

    [Test]
    public async Task Create_SetsInitialStateAndRecordsCreatedEvent()
    {
        var id = ReplyId.New();
        var authorId = ActorId.New();
        var target = ReplyTarget.ToPost(PostId.New());
        var content = PostContent.Create("Hello reply!").Value;
        var visibility = Visibility.Public;

        var result = Reply.Create(id, authorId, target, content, visibility, SampleCreatedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        var reply = result.Value;
        await Assert.That(reply.Id).IsEqualTo(id);
        await Assert.That(reply.AuthorId).IsEqualTo(authorId);
        await Assert.That(reply.Target).IsEqualTo(target);
        await Assert.That(reply.Content).IsEqualTo(content);
        await Assert.That(reply.Visibility).IsEqualTo(visibility);
        await Assert.That(reply.Status).IsEqualTo(ReplyStatus.Active);
        await Assert.That(reply.CreatedAt).IsEqualTo(SampleCreatedAt);
        await Assert.That(reply.EditedAt).IsNull();
        await Assert.That(reply.DomainEvents.Count).IsEqualTo(1);

        var ev = (ReplyCreated)reply.DomainEvents[0];
        await Assert.That(ev.ReplyId).IsEqualTo(id);
        await Assert.That(ev.AuthorId).IsEqualTo(authorId);
        await Assert.That(ev.Target).IsEqualTo(target);
        await Assert.That(ev.Text).IsEqualTo("Hello reply!");
        await Assert.That(ev.Visibility).IsEqualTo(visibility);
        await Assert.That(ev.CreatedAt).IsEqualTo(SampleCreatedAt);
    }

    [Test]
    public async Task Create_WithDefaultCreatedAt_ThrowsArgumentOutOfRangeException()
    {
        var id = ReplyId.New();
        var authorId = ActorId.New();
        var target = ReplyTarget.ToPost(PostId.New());
        var content = PostContent.Create("Hello").Value;

        await Assert
            .That(() => Reply.Create(id, authorId, target, content, Visibility.Public, default))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Create_WithUndefinedVisibility_ThrowsArgumentOutOfRangeException()
    {
        var id = ReplyId.New();
        var authorId = ActorId.New();
        var target = ReplyTarget.ToPost(PostId.New());
        var content = PostContent.Create("Hello").Value;

        await Assert
            .That(() =>
                Reply.Create(id, authorId, target, content, (Visibility)99, SampleCreatedAt)
            )
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Create_WithNullTarget_ThrowsArgumentNullException()
    {
        var id = ReplyId.New();
        var authorId = ActorId.New();
        var content = PostContent.Create("Hello").Value;

        await Assert
            .That(() =>
                Reply.Create(id, authorId, null!, content, Visibility.Public, SampleCreatedAt)
            )
            .Throws<ArgumentNullException>();
    }

    // --- Reconstitute ---

    [Test]
    public async Task Reconstitute_RestoresAllPropertiesWithoutEvents()
    {
        var id = ReplyId.New();
        var authorId = ActorId.New();
        var target = ReplyTarget.ToPost(PostId.New());
        var content = PostContent.Create("Reconstituted text").Value;
        var visibility = Visibility.Followers;

        var reply = Reply.Reconstitute(
            id,
            authorId,
            target,
            content,
            visibility,
            ReplyStatus.Deleted,
            SampleCreatedAt,
            SampleEditedAt
        );

        await Assert.That(reply.Id).IsEqualTo(id);
        await Assert.That(reply.AuthorId).IsEqualTo(authorId);
        await Assert.That(reply.Target).IsEqualTo(target);
        await Assert.That(reply.Content).IsEqualTo(content);
        await Assert.That(reply.Visibility).IsEqualTo(visibility);
        await Assert.That(reply.Status).IsEqualTo(ReplyStatus.Deleted);
        await Assert.That(reply.CreatedAt).IsEqualTo(SampleCreatedAt);
        await Assert.That(reply.EditedAt).IsEqualTo(SampleEditedAt);
        await Assert.That(reply.DomainEvents.Count).IsEqualTo(0);
    }

    // --- Edit ---

    [Test]
    public async Task Edit_UpdatesContentAndRecordsEditedEvent()
    {
        var reply = CreateSampleReply();
        var newContent = PostContent.Create("Updated reply text").Value;

        var result = reply.Edit(reply.AuthorId, newContent, SampleEditedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(reply.Content).IsEqualTo(newContent);
        await Assert.That(reply.EditedAt).IsEqualTo(SampleEditedAt);
        await Assert.That(reply.DomainEvents.Count).IsEqualTo(2);

        var ev = (ReplyEdited)reply.DomainEvents[1];
        await Assert.That(ev.ReplyId).IsEqualTo(reply.Id);
        await Assert.That(ev.AuthorId).IsEqualTo(reply.AuthorId);
        await Assert.That(ev.PreviousText).IsEqualTo("Sample reply text");
        await Assert.That(ev.CurrentText).IsEqualTo("Updated reply text");
        await Assert.That(ev.EditedAt).IsEqualTo(SampleEditedAt);
    }

    [Test]
    public async Task Edit_ByDifferentActor_ReturnsForbiddenError()
    {
        var reply = CreateSampleReply();
        var otherActor = ActorId.New();
        var newContent = PostContent.Create("Updated text").Value;

        var result = reply.Edit(otherActor, newContent, SampleEditedAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(ContentErrors.ReplyEditorMismatch);
        await Assert.That(reply.EditedAt).IsNull();
        await Assert.That(reply.DomainEvents.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Edit_WhenDeleted_ReturnsConflictError()
    {
        var reply = CreateSampleReply();
        reply.Delete(reply.AuthorId, SampleDeletedAt);
        var newContent = PostContent.Create("Updated text").Value;

        var result = reply.Edit(reply.AuthorId, newContent, SampleEditedAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(ContentErrors.ReplyNotEditable);
    }

    [Test]
    public async Task Edit_WithSameContent_IsIdempotentAndDoesNotRecordEvent()
    {
        var reply = CreateSampleReply();
        var sameContent = reply.Content;

        var result = reply.Edit(reply.AuthorId, sameContent, SampleEditedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(reply.EditedAt).IsNull();
        await Assert.That(reply.DomainEvents.Count).IsEqualTo(1);
    }

    // --- Delete ---

    [Test]
    public async Task Delete_SetsDeletedStatusAndRecordsDeletedEvent()
    {
        var reply = CreateSampleReply();

        var result = reply.Delete(reply.AuthorId, SampleDeletedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(reply.Status).IsEqualTo(ReplyStatus.Deleted);
        await Assert.That(reply.DomainEvents.Count).IsEqualTo(2);

        var ev = (ReplyDeleted)reply.DomainEvents[1];
        await Assert.That(ev.ReplyId).IsEqualTo(reply.Id);
        await Assert.That(ev.AuthorId).IsEqualTo(reply.AuthorId);
        await Assert.That(ev.DeletedAt).IsEqualTo(SampleDeletedAt);
    }

    [Test]
    public async Task Delete_ByDifferentActor_ReturnsForbiddenError()
    {
        var reply = CreateSampleReply();
        var otherActor = ActorId.New();

        var result = reply.Delete(otherActor, SampleDeletedAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(ContentErrors.ReplyEditorMismatch);
        await Assert.That(reply.Status).IsEqualTo(ReplyStatus.Active);
        await Assert.That(reply.DomainEvents.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Delete_WhenAlreadyDeleted_IsIdempotentAndDoesNotRecordDuplicateEvent()
    {
        var reply = CreateSampleReply();
        reply.Delete(reply.AuthorId, SampleDeletedAt);
        var countAfterFirstDelete = reply.DomainEvents.Count;

        var result = reply.Delete(reply.AuthorId, SampleDeletedAt.AddMinutes(1));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(reply.Status).IsEqualTo(ReplyStatus.Deleted);
        await Assert.That(reply.DomainEvents.Count).IsEqualTo(countAfterFirstDelete);
    }

    // --- Import / ImportDeletion ---

    [Test]
    public async Task Import_CreatesReplyWithReplyCreatedEvent()
    {
        var id = ReplyId.New();
        var authorId = ActorId.New();
        var target = ReplyTarget.ToPost(PostId.New());
        var content = PostContent.Create("Imported reply").Value;

        var result = Reply.Import(
            id,
            authorId,
            target,
            content,
            Visibility.Public,
            SampleCreatedAt
        );

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.DomainEvents.Count).IsEqualTo(1);
    }

    [Test]
    public async Task ImportDeletion_SetsDeletedStatusWithoutAuthorCheck()
    {
        var reply = CreateSampleReply();

        var result = reply.ImportDeletion(SampleDeletedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(reply.Status).IsEqualTo(ReplyStatus.Deleted);
        await Assert.That(reply.DomainEvents.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ImportDeletion_WhenAlreadyDeleted_IsIdempotent()
    {
        var reply = CreateSampleReply();
        reply.ImportDeletion(SampleDeletedAt);
        var count = reply.DomainEvents.Count;

        var result = reply.ImportDeletion(SampleDeletedAt.AddMinutes(1));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(reply.DomainEvents.Count).IsEqualTo(count);
    }
}
