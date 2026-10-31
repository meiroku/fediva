using Fediva.Domain.Content;

namespace Fediva.Domain.UnitTests.Content;

public class ReplyTargetTests
{
    [Test]
    public async Task ToPost_CreatesTargetWithRootAndParentAsPost()
    {
        var postId = PostId.New();
        var target = ReplyTarget.ToPost(postId);

        await Assert.That(target.Root).IsEqualTo(ContentRef.FromPost(postId));
        await Assert.That(target.Parent).IsEqualTo(ContentRef.FromPost(postId));
    }

    [Test]
    public async Task ToReply_CreatesTargetWithSpecifiedRootAndParentReply()
    {
        var rootPostId = PostId.New();
        var parentReplyId = ReplyId.New();
        var rootRef = ContentRef.FromPost(rootPostId);

        var target = ReplyTarget.ToReply(rootRef, parentReplyId);

        await Assert.That(target.Root).IsEqualTo(rootRef);
        await Assert.That(target.Parent).IsEqualTo(ContentRef.FromReply(parentReplyId));
    }

    [Test]
    public async Task Constructor_InitializesCorrectly()
    {
        var root = ContentRef.FromPost(PostId.New());
        var parent = ContentRef.FromReply(ReplyId.New());

        var target = new ReplyTarget(root, parent);

        await Assert.That(target.Root).IsEqualTo(root);
        await Assert.That(target.Parent).IsEqualTo(parent);
    }

    [Test]
    public async Task Equality_WhenSameRootAndParent_AreEqual()
    {
        var postId = PostId.New();
        var target1 = ReplyTarget.ToPost(postId);
        var target2 = ReplyTarget.ToPost(postId);

        await Assert.That(target1).IsEqualTo(target2);
    }
}
