using Fediva.Domain.Content;

namespace Fediva.Domain.UnitTests.Content;

public class ReplyIdTests
{
    [Test]
    public async Task New_CreatesUniqueUUIDv7Ids()
    {
        var id1 = ReplyId.New();
        var id2 = ReplyId.New();

        await Assert.That(id1).IsNotEqualTo(id2);
    }

    [Test]
    public async Task Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        await Assert.That(() => new ReplyId(Guid.Empty)).Throws<ArgumentException>();
    }

    [Test]
    public async Task Constructor_WithNonUUIDv7_ThrowsArgumentException()
    {
        var v4 = Guid.NewGuid();
        await Assert.That(() => new ReplyId(v4)).Throws<ArgumentException>();
    }

    [Test]
    public async Task CompareTo_OrdersByValue()
    {
        var id1 = ReplyId.New();
        await Task.Delay(1);
        var id2 = ReplyId.New();

        await Assert.That(id1.CompareTo(id2)).IsLessThan(0);
    }
}
