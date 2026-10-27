using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.UnitTests.Identity.Actors;

public class ActorValueObjectTests
{
    [Test]
    public async Task DisplayName_Create_TrimsValueAndEnforcesLengthConstraints()
    {
        var normalized = DisplayName.Create("  Fediva Persona  ");
        var exactMax = DisplayName.Create(new string('a', DisplayName.MaximumLength));
        var tooLong = DisplayName.Create(new string('a', DisplayName.MaximumLength + 1));

        await Assert.That(normalized.IsSuccess).IsTrue();
        await Assert.That(normalized.Value.Value).IsEqualTo("Fediva Persona");
        await Assert.That(exactMax.IsSuccess).IsTrue();
        await Assert.That(tooLong.IsFailure).IsTrue();
        await Assert.That(tooLong.Error).IsEqualTo(IdentityErrors.DisplayNameTooLong);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task DisplayName_Create_WithNullOrWhitespace_ReturnsDisplayNameRequired(
        string? value
    )
    {
        var result = DisplayName.Create(value);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(IdentityErrors.DisplayNameRequired);
    }

    [Test]
    public async Task DisplayName_WithSameValue_HasValueEquality()
    {
        var first = DisplayName.Create("Alice").Value;
        var second = DisplayName.Create("  Alice  ").Value;
        var different = DisplayName.Create("Bob").Value;

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
        await Assert.That(first).IsNotEqualTo(different);
    }

    [Test]
    public async Task ActorProfile_Create_NormalizesBioAndEnforcesMaximumLength()
    {
        var displayName = DisplayName.Create("Fediva Bot").Value;
        var profile = ActorProfile.Create(displayName, "  An automated assistant.  ");
        var maxBio = ActorProfile.Create(
            displayName,
            new string('b', ActorProfile.MaximumBioLength)
        );
        var tooLongBio = ActorProfile.Create(
            displayName,
            new string('b', ActorProfile.MaximumBioLength + 1)
        );

        await Assert.That(profile.IsSuccess).IsTrue();
        await Assert.That(profile.Value.DisplayName).IsEqualTo(displayName);
        await Assert.That(profile.Value.Bio).IsEqualTo("An automated assistant.");

        await Assert.That(maxBio.IsSuccess).IsTrue();
        await Assert.That(tooLongBio.IsFailure).IsTrue();
        await Assert.That(tooLongBio.Error).IsEqualTo(IdentityErrors.BioTooLong);
    }

    [Test]
    public async Task ActorProfile_Create_WithNullBio_NormalizesToEmptyString()
    {
        var displayName = DisplayName.Create("Fediva Bot").Value;
        var profile = ActorProfile.Create(displayName, null);

        await Assert.That(profile.IsSuccess).IsTrue();
        await Assert.That(profile.Value.Bio).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ActorProfile_Create_WithNullDisplayName_ThrowsArgumentNullException()
    {
        await Assert.That(() => ActorProfile.Create(null!, "bio")).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ActorProfile_WithSameValues_HasValueEquality()
    {
        var displayName = DisplayName.Create("Alice").Value;
        var first = ActorProfile.Create(displayName, "Hello").Value;
        var second = ActorProfile.Create(displayName, "  Hello  ").Value;
        var different = ActorProfile.Create(displayName, "Goodbye").Value;

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
        await Assert.That(first).IsNotEqualTo(different);
    }

    [Test]
    public async Task ActorKind_ContainsExpectedDefinedValues()
    {
        await Assert.That(Enum.IsDefined(ActorKind.Person)).IsTrue();
        await Assert.That(Enum.IsDefined(ActorKind.Bot)).IsTrue();
        await Assert.That(Enum.IsDefined(ActorKind.Organization)).IsTrue();
        await Assert.That(Enum.IsDefined(ActorKind.Service)).IsTrue();
        await Assert.That(Enum.IsDefined(ActorKind.Group)).IsTrue();
    }

    [Test]
    public async Task ActorOrigin_ContainsExpectedDefinedValues()
    {
        await Assert.That(Enum.IsDefined(ActorOrigin.Local)).IsTrue();
        await Assert.That(Enum.IsDefined(ActorOrigin.Remote)).IsTrue();
    }

    [Test]
    public async Task ActorStatus_ContainsExpectedDefinedValues()
    {
        await Assert.That(Enum.IsDefined(ActorStatus.Active)).IsTrue();
        await Assert.That(Enum.IsDefined(ActorStatus.Suspended)).IsTrue();
        await Assert.That(Enum.IsDefined(ActorStatus.Migrated)).IsTrue();
        await Assert.That(Enum.IsDefined(ActorStatus.Deleted)).IsTrue();
    }
}
