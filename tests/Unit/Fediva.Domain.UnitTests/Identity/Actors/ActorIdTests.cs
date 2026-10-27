using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.UnitTests.Identity.Actors;

public class ActorIdTests
{
    [Test]
    public async Task ActorId_WithSameGuid_HasValueEquality()
    {
        var value = Guid.CreateVersion7();
        var first = new ActorId(value);
        var second = new ActorId(value);

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task ActorId_New_GeneratesUniqueIds()
    {
        var first = ActorId.New();
        var second = ActorId.New();

        await Assert.That(first).IsNotEqualTo(second);
    }

    [Test]
    public async Task ActorId_WithEmptyGuid_ThrowsArgumentException()
    {
        await Assert.That(() => new ActorId(Guid.Empty)).Throws<ArgumentException>();
    }

    [Test]
    public async Task ActorId_WithVersion4Guid_ThrowsArgumentException()
    {
        // ActorIdはソート可能性と時系列順を保証するためUUIDv7のみを受け付ける
        await Assert.That(() => new ActorId(Guid.NewGuid())).Throws<ArgumentException>();
    }

    [Test]
    public async Task ActorId_CompareTo_ReturnsExpectedOrder()
    {
        var first = ActorId.New();
        var second = ActorId.New();

        await Assert.That(first.CompareTo(first)).IsEqualTo(0);
        await Assert.That(first.CompareTo(second)).IsEqualTo(first.Value.CompareTo(second.Value));
    }
}
