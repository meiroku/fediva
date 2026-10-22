using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Users;

namespace Fediva.Domain.UnitTests.Identity.Users;

public class UserValueObjectTests
{
    [Test]
    public async Task UserId_WithSameGuid_HasValueEquality()
    {
        var value = Guid.CreateVersion7();
        var first = new UserId(value);
        var second = new UserId(value);

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task UserId_New_GeneratesUniqueIds()
    {
        var first = UserId.New();
        var second = UserId.New();

        await Assert.That(first).IsNotEqualTo(second);
    }

    [Test]
    public async Task UserId_WithEmptyGuid_ThrowsArgumentException()
    {
        await Assert.That(() => new UserId(Guid.Empty)).Throws<ArgumentException>();
    }

    [Test]
    public async Task UserId_WithVersion4Guid_ThrowsArgumentException()
    {
        // UserIdはソート可能性を保証するためUUIDv7のみを受け付ける
        await Assert.That(() => new UserId(Guid.NewGuid())).Throws<ArgumentException>();
    }

    [Test]
    public async Task EmailAddress_Create_NormalizesAndComparesAddressesCaseInsensitively()
    {
        var first = EmailAddress.Create("  Person@Example.com ");
        var second = EmailAddress.Create("person@example.com");

        await Assert.That(first.IsSuccess).IsTrue();
        await Assert.That(first.Value.Value).IsEqualTo("person@example.com");
        await Assert.That(first.Value).IsEqualTo(second.Value);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("not-an-email")]
    [Arguments("person@example.com, other@example.com")]
    public async Task EmailAddress_Create_WithInvalidAddress_ReturnsEmailInvalid(string? value)
    {
        var result = EmailAddress.Create(value);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(IdentityErrors.EmailInvalid);
    }

    [Test]
    public async Task DisplayName_Create_TrimsValueAndEnforcesMaximumLength()
    {
        var normalized = DisplayName.Create("  Fediva User  ");
        var maximumLength = DisplayName.Create(new string('a', 30));
        var tooLong = DisplayName.Create(new string('a', 31));

        await Assert.That(normalized.IsSuccess).IsTrue();
        await Assert.That(normalized.Value.Value).IsEqualTo("Fediva User");
        await Assert.That(maximumLength.IsSuccess).IsTrue();
        await Assert.That(tooLong.IsFailure).IsTrue();
        await Assert.That(tooLong.Error).IsEqualTo(IdentityErrors.DisplayNameTooLong);
    }
}
