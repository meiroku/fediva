using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.UnitTests.Identity.Actors;

public class ActorTests
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

    private static Actor CreateSampleActor(
        ActorKind kind = ActorKind.Person,
        ActorOrigin origin = ActorOrigin.Local
    )
    {
        var id = ActorId.New();
        var displayName = DisplayName.Create("Test Actor").Value;
        var profile = ActorProfile.Create(displayName, "Sample bio").Value;
        return Actor.Create(id, kind, origin, profile, SampleCreatedAt);
    }

    [Test]
    public async Task Create_SetsInitialStateAndRecordsCreatedEvent()
    {
        var id = ActorId.New();
        var kind = ActorKind.Person;
        var origin = ActorOrigin.Local;
        var displayName = DisplayName.Create("Alice").Value;
        var profile = ActorProfile.Create(displayName, "Hello world").Value;
        var createdAt = SampleCreatedAt;

        var actor = Actor.Create(id, kind, origin, profile, createdAt);

        await Assert.That(actor.Id).IsEqualTo(id);
        await Assert.That(actor.Kind).IsEqualTo(kind);
        await Assert.That(actor.Origin).IsEqualTo(origin);
        await Assert.That(actor.Status).IsEqualTo(ActorStatus.Active);
        await Assert.That(actor.Profile).IsEqualTo(profile);
        await Assert.That(actor.CreatedAt).IsEqualTo(createdAt);
        await Assert.That(actor.MovedToActorId).IsNull();
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(1);

        var createdEvent = (ActorCreated)actor.DomainEvents[0];
        await Assert.That(createdEvent.ActorId).IsEqualTo(id);
        await Assert.That(createdEvent.Kind).IsEqualTo(kind);
        await Assert.That(createdEvent.Origin).IsEqualTo(origin);
        await Assert.That(createdEvent.Profile).IsEqualTo(profile);
    }

    [Test]
    public async Task Create_WithNullProfile_ThrowsArgumentNullException()
    {
        var id = ActorId.New();
        await Assert
            .That(() =>
                Actor.Create(id, ActorKind.Person, ActorOrigin.Local, null!, SampleCreatedAt)
            )
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Create_WithInvalidKind_ThrowsArgumentOutOfRangeException()
    {
        var id = ActorId.New();
        var profile = ActorProfile.Create(DisplayName.Create("A").Value).Value;
        await Assert
            .That(() =>
                Actor.Create(id, (ActorKind)999, ActorOrigin.Local, profile, SampleCreatedAt)
            )
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Create_WithInvalidOrigin_ThrowsArgumentOutOfRangeException()
    {
        var id = ActorId.New();
        var profile = ActorProfile.Create(DisplayName.Create("A").Value).Value;
        await Assert
            .That(() =>
                Actor.Create(id, ActorKind.Person, (ActorOrigin)999, profile, SampleCreatedAt)
            )
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Create_WithDefaultCreatedAt_ThrowsArgumentOutOfRangeException()
    {
        var id = ActorId.New();
        var profile = ActorProfile.Create(DisplayName.Create("A").Value).Value;
        await Assert
            .That(() => Actor.Create(id, ActorKind.Person, ActorOrigin.Local, profile, default))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Reconstitute_RestoresStateWithoutRecordingAnyDomainEvents()
    {
        var id = ActorId.New();
        var kind = ActorKind.Bot;
        var origin = ActorOrigin.Remote;
        var status = ActorStatus.Suspended;
        var profile = ActorProfile.Create(DisplayName.Create("Bot").Value, "Remote Bot").Value;
        var movedTo = ActorId.New();

        var actor = Actor.Reconstitute(id, kind, origin, status, profile, SampleCreatedAt, movedTo);

        await Assert.That(actor.Id).IsEqualTo(id);
        await Assert.That(actor.Kind).IsEqualTo(kind);
        await Assert.That(actor.Origin).IsEqualTo(origin);
        await Assert.That(actor.Status).IsEqualTo(status);
        await Assert.That(actor.Profile).IsEqualTo(profile);
        await Assert.That(actor.CreatedAt).IsEqualTo(SampleCreatedAt);
        await Assert.That(actor.MovedToActorId).IsEqualTo(movedTo);
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(0);
    }

    [Test]
    public async Task UpdateProfile_UpdatesProfileAndRecordsEvent()
    {
        var actor = CreateSampleActor();
        var previousProfile = actor.Profile;
        var newProfile = ActorProfile
            .Create(DisplayName.Create("New Name").Value, "Updated bio")
            .Value;

        var result = actor.UpdateProfile(newProfile);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(actor.Profile).IsEqualTo(newProfile);
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(2);

        var changedEvent = (ActorProfileChanged)actor.DomainEvents[1];
        await Assert.That(changedEvent.ActorId).IsEqualTo(actor.Id);
        await Assert.That(changedEvent.Previous).IsEqualTo(previousProfile);
        await Assert.That(changedEvent.Current).IsEqualTo(newProfile);
    }

    [Test]
    public async Task UpdateProfile_WithSameProfile_DoesNotRecordAnotherEvent()
    {
        var actor = CreateSampleActor();
        var result = actor.UpdateProfile(actor.Profile);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(1);
    }

    [Test]
    public async Task UpdateProfile_WhenDeleted_ReturnsFailure()
    {
        var actor = CreateSampleActor();
        actor.Delete();

        var newProfile = ActorProfile.Create(DisplayName.Create("New Name").Value).Value;
        var result = actor.UpdateProfile(newProfile);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(IdentityErrors.ActorStatusTransitionInvalid);
    }

    [Test]
    public async Task UpdateProfile_WhenMigrated_ReturnsFailure()
    {
        var actor = CreateSampleActor();
        actor.Migrate(ActorId.New());

        var newProfile = ActorProfile.Create(DisplayName.Create("New Name").Value).Value;
        var result = actor.UpdateProfile(newProfile);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(IdentityErrors.ActorStatusTransitionInvalid);
    }

    [Test]
    public async Task Suspend_UpdatesStatusAndRecordsEvent()
    {
        var actor = CreateSampleActor();
        var result = actor.Suspend();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(actor.Status).IsEqualTo(ActorStatus.Suspended);
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(2);

        var changedEvent = (ActorStatusChanged)actor.DomainEvents[1];
        await Assert.That(changedEvent.ActorId).IsEqualTo(actor.Id);
        await Assert.That(changedEvent.Previous).IsEqualTo(ActorStatus.Active);
        await Assert.That(changedEvent.Current).IsEqualTo(ActorStatus.Suspended);
    }

    [Test]
    public async Task Suspend_WhenAlreadySuspended_DoesNotRecordAnotherEvent()
    {
        var actor = CreateSampleActor();
        actor.Suspend();
        var countBefore = actor.DomainEvents.Count;

        var result = actor.Suspend();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(countBefore);
    }

    [Test]
    public async Task Activate_UpdatesStatusAndRecordsEvent()
    {
        var actor = CreateSampleActor();
        actor.Suspend();
        var result = actor.Activate();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(actor.Status).IsEqualTo(ActorStatus.Active);
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(3);

        var changedEvent = (ActorStatusChanged)actor.DomainEvents[2];
        await Assert.That(changedEvent.Previous).IsEqualTo(ActorStatus.Suspended);
        await Assert.That(changedEvent.Current).IsEqualTo(ActorStatus.Active);
    }

    [Test]
    public async Task Activate_WhenAlreadyActive_DoesNotRecordAnotherEvent()
    {
        var actor = CreateSampleActor();
        var result = actor.Activate();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Delete_UpdatesStatusAndRecordsEvent()
    {
        var actor = CreateSampleActor();
        var result = actor.Delete();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(actor.Status).IsEqualTo(ActorStatus.Deleted);
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(2);

        var changedEvent = (ActorStatusChanged)actor.DomainEvents[1];
        await Assert.That(changedEvent.Previous).IsEqualTo(ActorStatus.Active);
        await Assert.That(changedEvent.Current).IsEqualTo(ActorStatus.Deleted);
    }

    [Test]
    public async Task Delete_WhenAlreadyDeleted_DoesNotRecordAnotherEvent()
    {
        var actor = CreateSampleActor();
        actor.Delete();
        var countBefore = actor.DomainEvents.Count;

        var result = actor.Delete();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(countBefore);
    }

    [Test]
    public async Task Delete_FromSuspended_Succeeds()
    {
        var actor = CreateSampleActor();
        actor.Suspend();
        var result = actor.Delete();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(actor.Status).IsEqualTo(ActorStatus.Deleted);
    }

    [Test]
    public async Task Migrate_UpdatesStatusAndRecordsBothStatusChangedAndMigratedEvents()
    {
        var actor = CreateSampleActor();
        var destinationId = ActorId.New();

        var result = actor.Migrate(destinationId);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(actor.Status).IsEqualTo(ActorStatus.Migrated);
        await Assert.That(actor.MovedToActorId).IsEqualTo(destinationId);
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(3);

        var statusEvent = (ActorStatusChanged)actor.DomainEvents[1];
        await Assert.That(statusEvent.Previous).IsEqualTo(ActorStatus.Active);
        await Assert.That(statusEvent.Current).IsEqualTo(ActorStatus.Migrated);

        var migratedEvent = (ActorMigrated)actor.DomainEvents[2];
        await Assert.That(migratedEvent.ActorId).IsEqualTo(actor.Id);
        await Assert.That(migratedEvent.DestinationActorId).IsEqualTo(destinationId);
    }

    [Test]
    public async Task Migrate_ToSameActorId_ReturnsFailure()
    {
        var actor = CreateSampleActor();
        var result = actor.Migrate(actor.Id);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(IdentityErrors.ActorMigrationDestinationInvalid);
        await Assert.That(actor.Status).IsEqualTo(ActorStatus.Active);
        await Assert.That(actor.DomainEvents.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Migrate_FromSuspended_ReturnsFailure()
    {
        var actor = CreateSampleActor();
        actor.Suspend();

        var result = actor.Migrate(ActorId.New());

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(IdentityErrors.ActorStatusTransitionInvalid);
        await Assert.That(actor.Status).IsEqualTo(ActorStatus.Suspended);
    }

    [Test]
    public async Task StateTransitions_FromTerminalDeletedState_AreRejected()
    {
        var actor = CreateSampleActor();
        actor.Delete();

        var suspendResult = actor.Suspend();
        var activateResult = actor.Activate();
        var migrateResult = actor.Migrate(ActorId.New());

        await Assert.That(suspendResult.IsFailure).IsTrue();
        await Assert
            .That(suspendResult.Error)
            .IsEqualTo(IdentityErrors.ActorStatusTransitionInvalid);

        await Assert.That(activateResult.IsFailure).IsTrue();
        await Assert
            .That(activateResult.Error)
            .IsEqualTo(IdentityErrors.ActorStatusTransitionInvalid);

        await Assert.That(migrateResult.IsFailure).IsTrue();
        await Assert
            .That(migrateResult.Error)
            .IsEqualTo(IdentityErrors.ActorStatusTransitionInvalid);
    }

    [Test]
    public async Task StateTransitions_FromTerminalMigratedState_AreRejected()
    {
        var actor = CreateSampleActor();
        actor.Migrate(ActorId.New());

        var suspendResult = actor.Suspend();
        var activateResult = actor.Activate();
        var deleteResult = actor.Delete();
        var migrateAgainResult = actor.Migrate(ActorId.New());

        await Assert.That(suspendResult.IsFailure).IsTrue();
        await Assert
            .That(suspendResult.Error)
            .IsEqualTo(IdentityErrors.ActorStatusTransitionInvalid);

        await Assert.That(activateResult.IsFailure).IsTrue();
        await Assert
            .That(activateResult.Error)
            .IsEqualTo(IdentityErrors.ActorStatusTransitionInvalid);

        await Assert.That(deleteResult.IsFailure).IsTrue();
        await Assert
            .That(deleteResult.Error)
            .IsEqualTo(IdentityErrors.ActorStatusTransitionInvalid);

        await Assert.That(migrateAgainResult.IsFailure).IsTrue();
        await Assert
            .That(migrateAgainResult.Error)
            .IsEqualTo(IdentityErrors.ActorStatusTransitionInvalid);
    }
}
