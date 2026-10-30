using Fediva.Domain.SocialGraph;

namespace Fediva.Domain.UnitTests.SocialGraph;

public class FollowIdTests
{
    [Test]
    public async Task New_CreatesUniqueUUIDv7Ids()
    {
        var id1 = FollowId.New();
        var id2 = FollowId.New();

        await Assert.That(id1).IsNotEqualTo(id2);
    }

    [Test]
    public async Task Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        await Assert.That(() => new FollowId(Guid.Empty)).Throws<ArgumentException>();
    }

    [Test]
    public async Task Constructor_WithNonUUIDv7_ThrowsArgumentException()
    {
        var v4 = Guid.NewGuid();
        await Assert.That(() => new FollowId(v4)).Throws<ArgumentException>();
    }
}
