using Fediva.Domain.Common.Errors;
using Fediva.Domain.Content;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.UnitTests.Content;

public class PostTests
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

    private static Post CreateSamplePost(
        ActorId? authorId = null,
        Visibility visibility = Visibility.Public
    )
    {
        var id = PostId.New();
        var author = authorId ?? ActorId.New();
        var content = PostContent.Create("Sample post text").Value;
        return Post.Create(id, author, content, visibility, SampleCreatedAt).Value;
    }

    // --- Create ---

    [Test]
    public async Task Create_SetsInitialStateAndRecordsCreatedEvent()
    {
        var id = PostId.New();
        var authorId = ActorId.New();
        var content = PostContent.Create("Hello!").Value;
        var visibility = Visibility.Public;

        var result = Post.Create(id, authorId, content, visibility, SampleCreatedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        var post = result.Value;
        await Assert.That(post.Id).IsEqualTo(id);
        await Assert.That(post.AuthorId).IsEqualTo(authorId);
        await Assert.That(post.Content).IsEqualTo(content);
        await Assert.That(post.Visibility).IsEqualTo(visibility);
        await Assert.That(post.Status).IsEqualTo(PostStatus.Active);
        await Assert.That(post.CreatedAt).IsEqualTo(SampleCreatedAt);
        await Assert.That(post.EditedAt).IsNull();
        await Assert.That(post.QuoteReference).IsNull();
        await Assert.That(post.DomainEvents.Count).IsEqualTo(1);

        var ev = (PostCreated)post.DomainEvents[0];
        await Assert.That(ev.PostId).IsEqualTo(id);
        await Assert.That(ev.AuthorId).IsEqualTo(authorId);
        await Assert.That(ev.Text).IsEqualTo("Hello!");
        await Assert.That(ev.Visibility).IsEqualTo(visibility);
        await Assert.That(ev.QuotedPostId).IsNull();
        await Assert.That(ev.CreatedAt).IsEqualTo(SampleCreatedAt);
    }

    [Test]
    public async Task Create_WithQuoteReference_SetsQuoteReferenceAndIncludesInEvent()
    {
        var quotedId = PostId.New();
        var quoteRef = new QuoteReference(quotedId);
        var id = PostId.New();
        var authorId = ActorId.New();
        var content = PostContent.Create("Quote post").Value;

        var result = Post.Create(
            id,
            authorId,
            content,
            Visibility.Public,
            SampleCreatedAt,
            quoteRef
        );

        await Assert.That(result.IsSuccess).IsTrue();
        var post = result.Value;
        await Assert.That(post.QuoteReference).IsNotNull();
        await Assert.That(post.QuoteReference!.QuotedPostId).IsEqualTo(quotedId);

        var ev = (PostCreated)post.DomainEvents[0];
        await Assert.That(ev.QuotedPostId).IsEqualTo(quotedId);
    }

    [Test]
    public async Task Create_WithDefaultCreatedAt_ThrowsArgumentOutOfRangeException()
    {
        var id = PostId.New();
        var authorId = ActorId.New();
        var content = PostContent.Create("text").Value;

        await Assert
            .That(() => Post.Create(id, authorId, content, Visibility.Public, default))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Create_WithNullContent_ThrowsArgumentNullException()
    {
        var id = PostId.New();
        var authorId = ActorId.New();

        await Assert
            .That(() => Post.Create(id, authorId, null!, Visibility.Public, SampleCreatedAt))
            .Throws<ArgumentNullException>();
    }

    // --- Reconstitute ---

    [Test]
    public async Task Reconstitute_RestoresStateWithoutRecordingAnyDomainEvents()
    {
        var id = PostId.New();
        var authorId = ActorId.New();
        var content = PostContent.Create("Restored text").Value;
        var editedAt = SampleEditedAt;

        var post = Post.Reconstitute(
            id,
            authorId,
            content,
            Visibility.Followers,
            quoteReference: null,
            PostStatus.Active,
            SampleCreatedAt,
            editedAt
        );

        await Assert.That(post.Id).IsEqualTo(id);
        await Assert.That(post.AuthorId).IsEqualTo(authorId);
        await Assert.That(post.Content).IsEqualTo(content);
        await Assert.That(post.Visibility).IsEqualTo(Visibility.Followers);
        await Assert.That(post.Status).IsEqualTo(PostStatus.Active);
        await Assert.That(post.EditedAt).IsEqualTo(editedAt);
        await Assert.That(post.DomainEvents.Count).IsEqualTo(0);
    }

    // --- Edit ---

    [Test]
    public async Task Edit_UpdatesContentAndRecordsEvent()
    {
        var authorId = ActorId.New();
        var post = CreateSamplePost(authorId);
        var previousText = post.Content.Text;
        var newContent = PostContent.Create("Updated text").Value;

        var result = post.Edit(authorId, newContent, Visibility.Followers, SampleEditedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(post.Content).IsEqualTo(newContent);
        await Assert.That(post.Visibility).IsEqualTo(Visibility.Followers);
        await Assert.That(post.EditedAt).IsEqualTo(SampleEditedAt);
        await Assert.That(post.DomainEvents.Count).IsEqualTo(2);

        var ev = (PostEdited)post.DomainEvents[1];
        await Assert.That(ev.PostId).IsEqualTo(post.Id);
        await Assert.That(ev.AuthorId).IsEqualTo(authorId);
        await Assert.That(ev.PreviousText).IsEqualTo(previousText);
        await Assert.That(ev.CurrentText).IsEqualTo("Updated text");
        await Assert.That(ev.EditedAt).IsEqualTo(SampleEditedAt);
    }

    [Test]
    public async Task Edit_WithSameContent_IsIdempotentAndDoesNotEmitEvent()
    {
        var authorId = ActorId.New();
        var post = CreateSamplePost(authorId);
        var originalContent = post.Content;
        var countBefore = post.DomainEvents.Count;

        var result = post.Edit(authorId, originalContent, Visibility.Public, SampleEditedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(post.DomainEvents.Count).IsEqualTo(countBefore);
    }

    [Test]
    public async Task Edit_ByNonAuthor_ReturnsFailure()
    {
        var authorId = ActorId.New();
        var otherId = ActorId.New();
        var post = CreateSamplePost(authorId);
        var newContent = PostContent.Create("Hacked!").Value;

        var result = post.Edit(otherId, newContent, Visibility.Public, SampleEditedAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(ContentErrors.PostEditorMismatch);
    }

    [Test]
    public async Task Edit_WhenDeleted_ReturnsFailure()
    {
        var authorId = ActorId.New();
        var post = CreateSamplePost(authorId);
        post.Delete(authorId, SampleDeletedAt);
        var newContent = PostContent.Create("Edit after delete").Value;

        var result = post.Edit(authorId, newContent, Visibility.Public, SampleEditedAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(ContentErrors.PostNotEditable);
    }

    // --- Delete ---

    [Test]
    public async Task Delete_UpdatesStatusAndRecordsEvent()
    {
        var authorId = ActorId.New();
        var post = CreateSamplePost(authorId);

        var result = post.Delete(authorId, SampleDeletedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(post.Status).IsEqualTo(PostStatus.Deleted);
        await Assert.That(post.DomainEvents.Count).IsEqualTo(2);

        var ev = (PostDeleted)post.DomainEvents[1];
        await Assert.That(ev.PostId).IsEqualTo(post.Id);
        await Assert.That(ev.AuthorId).IsEqualTo(authorId);
        await Assert.That(ev.DeletedAt).IsEqualTo(SampleDeletedAt);
    }

    [Test]
    public async Task Delete_WhenAlreadyDeleted_IsIdempotentAndDoesNotEmitEvent()
    {
        var authorId = ActorId.New();
        var post = CreateSamplePost(authorId);
        post.Delete(authorId, SampleDeletedAt);
        var countBefore = post.DomainEvents.Count;

        var result = post.Delete(authorId, SampleDeletedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(post.DomainEvents.Count).IsEqualTo(countBefore);
    }

    [Test]
    public async Task Delete_ByNonAuthor_ReturnsFailure()
    {
        var authorId = ActorId.New();
        var otherId = ActorId.New();
        var post = CreateSamplePost(authorId);

        var result = post.Delete(otherId, SampleDeletedAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(ContentErrors.PostEditorMismatch);
        await Assert.That(post.Status).IsEqualTo(PostStatus.Active);
    }

    // --- ImportDeletion ---

    [Test]
    public async Task ImportDeletion_WhenActive_DeletesAndRecordsEvent()
    {
        var authorId = ActorId.New();
        var post = CreateSamplePost(authorId);

        var result = post.ImportDeletion(SampleDeletedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(post.Status).IsEqualTo(PostStatus.Deleted);
        await Assert.That(post.DomainEvents.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ImportDeletion_WhenAlreadyDeleted_IsIdempotent()
    {
        var authorId = ActorId.New();
        var post = CreateSamplePost(authorId);
        post.Delete(authorId, SampleDeletedAt);
        var countBefore = post.DomainEvents.Count;

        var result = post.ImportDeletion(SampleDeletedAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(post.DomainEvents.Count).IsEqualTo(countBefore);
    }

    // --- Visibility ---

    [Test]
    [Arguments(Visibility.Public)]
    [Arguments(Visibility.Followers)]
    [Arguments(Visibility.Direct)]
    public async Task Create_WithValidVisibility_Succeeds(Visibility visibility)
    {
        var post = CreateSamplePost(visibility: visibility);
        await Assert.That(post.Visibility).IsEqualTo(visibility);
    }
}
