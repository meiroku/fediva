using Fediva.Domain.Identity.Users;

namespace Fediva.Domain.UnitTests.Identity.Users;

public class UserTests
{
    [Test]
    public async Task Create_SetsInitialStateAndRecordsCreatedEvent()
    {
        var id = UserId.New();
        var displayName = DisplayName.Create("Fediva User").Value;
        var emailAddress = EmailAddress.Create("user@example.com").Value;
        var createdAt = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.FromHours(2));
        var user = User.Create(id, displayName, emailAddress, createdAt);

        await Assert.That(user.Id).IsEqualTo(id);
        await Assert.That(user.DisplayName).IsEqualTo(displayName);
        await Assert.That(user.EmailAddress).IsEqualTo(emailAddress);
        await Assert.That(user.Status).IsEqualTo(UserStatus.Active);
        await Assert.That(user.CreatedAt).IsEqualTo(createdAt);
        await Assert.That(user.DomainEvents.Count).IsEqualTo(1);

        var createdEvent = (UserCreated)user.DomainEvents[0];
        await Assert.That(createdEvent.UserId).IsEqualTo(id);
        await Assert.That(createdEvent.DisplayName).IsEqualTo(displayName);
        await Assert.That(createdEvent.EmailAddress).IsEqualTo(emailAddress);
    }

    [Test]
    public async Task ChangeDisplayName_UpdatesStateAndRecordsEvent()
    {
        var user = CreateUser();
        var previous = user.DisplayName;
        var result = user.ChangeDisplayName(DisplayName.Create("  New Name  ").Value);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(user.DisplayName.Value).IsEqualTo("New Name");
        await Assert.That(user.DomainEvents.Count).IsEqualTo(2);

        var changedEvent = (DisplayNameChanged)user.DomainEvents[1];
        await Assert.That(changedEvent.Previous).IsEqualTo(previous);
        await Assert.That(changedEvent.Current).IsEqualTo(user.DisplayName);
    }

    [Test]
    public async Task ChangeEmailAddress_UpdatesStateAndRecordsEvent()
    {
        var user = CreateUser();
        var previous = user.EmailAddress;
        var result = user.ChangeEmailAddress(EmailAddress.Create("new@example.com").Value);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(user.EmailAddress.Value).IsEqualTo("new@example.com");
        await Assert.That(user.DomainEvents.Count).IsEqualTo(2);

        var changedEvent = (EmailAddressChanged)user.DomainEvents[1];
        await Assert.That(changedEvent.Previous).IsEqualTo(previous);
        await Assert.That(changedEvent.Current).IsEqualTo(user.EmailAddress);
    }

    [Test]
    public async Task ChangeDisplayName_WithSameValue_DoesNotRecordAnotherEvent()
    {
        var user = CreateUser();
        var result = user.ChangeDisplayName(user.DisplayName);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(user.DomainEvents.Count).IsEqualTo(1);
    }

    [Test]
    public async Task ChangeEmailAddress_WithSameValue_DoesNotRecordAnotherEvent()
    {
        var user = CreateUser();
        var result = user.ChangeEmailAddress(user.EmailAddress);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(user.DomainEvents.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Suspend_UpdatesStateAndRecordsEvent()
    {
        var user = CreateUser();
        var result = user.Suspend();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(user.Status).IsEqualTo(UserStatus.Suspended);
        await Assert.That(user.DomainEvents.Count).IsEqualTo(2);

        var changedEvent = (UserStatusChanged)user.DomainEvents[1];
        await Assert.That(changedEvent.Previous).IsEqualTo(UserStatus.Active);
        await Assert.That(changedEvent.Current).IsEqualTo(UserStatus.Suspended);
    }

    [Test]
    public async Task SuspendActivateAndDelete_UpdatesStateAndRecordsEachEvent()
    {
        var user = CreateUser();

        var suspendResult = user.Suspend();
        var restoreResult = user.Activate();
        var deleteResult = user.Delete();

        await Assert.That(suspendResult.IsSuccess).IsTrue();
        await Assert.That(restoreResult.IsSuccess).IsTrue();
        await Assert.That(deleteResult.IsSuccess).IsTrue();
        await Assert.That(user.Status).IsEqualTo(UserStatus.Deleted);
        await Assert.That(user.DomainEvents.Count).IsEqualTo(4);

        var suspendEvent = (UserStatusChanged)user.DomainEvents[1];
        var restoreEvent = (UserStatusChanged)user.DomainEvents[2];
        var deleteEvent = (UserStatusChanged)user.DomainEvents[3];
        await Assert.That(suspendEvent.Previous).IsEqualTo(UserStatus.Active);
        await Assert.That(suspendEvent.Current).IsEqualTo(UserStatus.Suspended);
        await Assert.That(restoreEvent.Previous).IsEqualTo(UserStatus.Suspended);
        await Assert.That(restoreEvent.Current).IsEqualTo(UserStatus.Active);
        await Assert.That(deleteEvent.Previous).IsEqualTo(UserStatus.Active);
        await Assert.That(deleteEvent.Current).IsEqualTo(UserStatus.Deleted);
    }

    [Test]
    public async Task Activate_WhenAlreadyActive_DoesNotRecordAnotherEvent()
    {
        var user = CreateUser();
        var result = user.Activate();

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(user.DomainEvents.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Activate_FromDeleted_IsRejectedWithoutChangingState()
    {
        var user = CreateUser();
        user.Delete();
        var eventCount = user.DomainEvents.Count;

        var result = user.Activate();

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(user.Status).IsEqualTo(UserStatus.Deleted);
        await Assert.That(user.DomainEvents.Count).IsEqualTo(eventCount);
    }

    [Test]
    public async Task Delete_FromWithdrawn_IsRejectedWithoutChangingState()
    {
        var user = CreateUser();
        user.Withdraw();
        var eventCount = user.DomainEvents.Count;

        var result = user.Delete();

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(user.Status).IsEqualTo(UserStatus.Withdrawn);
        await Assert.That(user.DomainEvents.Count).IsEqualTo(eventCount);
    }

    private static User CreateUser() =>
        User.Create(
            UserId.New(),
            DisplayName.Create("Fediva User").Value,
            EmailAddress.Create("user@example.com").Value,
            DateTimeOffset.UtcNow
        );
}
