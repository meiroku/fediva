using Fediva.Domain.Common.Errors;
using Fediva.Domain.Identity.Actors;
using Fediva.Domain.SocialGraph;

namespace Fediva.Domain.UnitTests.SocialGraph;

public class FollowRelationTests
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
    private static readonly DateTimeOffset SampleOccurredAt = new(
        2025,
        1,
        2,
        4,
        0,
        0,
        TimeSpan.Zero
    );

    private static (ActorId FollowerId, ActorId FolloweeId) CreateActorPair() =>
        (ActorId.New(), ActorId.New());

    private static FollowRelation CreateAcceptedFollow(
        ActorId? followerId = null,
        ActorId? followeeId = null
    )
    {
        var fid = followerId ?? ActorId.New();
        var eid = followeeId ?? ActorId.New();
        var result = FollowRelation.Follow(
            FollowId.New(),
            fid,
            eid,
            requiresApproval: false,
            SampleCreatedAt
        );
        return result.Value;
    }

    private static FollowRelation CreatePendingFollow(
        ActorId? followerId = null,
        ActorId? followeeId = null
    )
    {
        var fid = followerId ?? ActorId.New();
        var eid = followeeId ?? ActorId.New();
        var result = FollowRelation.Follow(
            FollowId.New(),
            fid,
            eid,
            requiresApproval: true,
            SampleCreatedAt
        );
        return result.Value;
    }

    // --- Follow (Create) ---

    [Test]
    public async Task Follow_WithoutApproval_CreatesAcceptedRelationAndRecordsEvent()
    {
        var (followerId, followeeId) = CreateActorPair();
        var id = FollowId.New();

        var result = FollowRelation.Follow(
            id,
            followerId,
            followeeId,
            requiresApproval: false,
            SampleCreatedAt
        );

        await Assert.That(result.IsSuccess).IsTrue();
        var relation = result.Value;
        await Assert.That(relation.Id).IsEqualTo(id);
        await Assert.That(relation.FollowerId).IsEqualTo(followerId);
        await Assert.That(relation.FolloweeId).IsEqualTo(followeeId);
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Accepted);
        await Assert.That(relation.CreatedAt).IsEqualTo(SampleCreatedAt);
        await Assert.That(relation.DomainEvents.Count).IsEqualTo(1);

        var ev = (FollowRequested)relation.DomainEvents[0];
        await Assert.That(ev.FollowId).IsEqualTo(id);
        await Assert.That(ev.FollowerId).IsEqualTo(followerId);
        await Assert.That(ev.FolloweeId).IsEqualTo(followeeId);
        await Assert.That(ev.InitialStatus).IsEqualTo(FollowStatus.Accepted);
    }

    [Test]
    public async Task Follow_WithApproval_CreatesPendingRelationAndRecordsEvent()
    {
        var (followerId, followeeId) = CreateActorPair();

        var result = FollowRelation.Follow(
            FollowId.New(),
            followerId,
            followeeId,
            requiresApproval: true,
            SampleCreatedAt
        );

        await Assert.That(result.IsSuccess).IsTrue();
        var relation = result.Value;
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Pending);

        var ev = (FollowRequested)relation.DomainEvents[0];
        await Assert.That(ev.InitialStatus).IsEqualTo(FollowStatus.Pending);
    }

    [Test]
    public async Task Follow_WithSameActorIdAsFollowerAndFollowee_ReturnsFailure()
    {
        var actorId = ActorId.New();

        var result = FollowRelation.Follow(
            FollowId.New(),
            actorId,
            actorId,
            requiresApproval: false,
            SampleCreatedAt
        );

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(SocialGraphErrors.SelfFollowNotAllowed);
    }

    // --- Reconstitute ---

    [Test]
    public async Task Reconstitute_RestoresStateWithoutRecordingAnyDomainEvents()
    {
        var (followerId, followeeId) = CreateActorPair();
        var id = FollowId.New();

        var relation = FollowRelation.Reconstitute(
            id,
            followerId,
            followeeId,
            FollowStatus.Accepted,
            SampleCreatedAt
        );

        await Assert.That(relation.Id).IsEqualTo(id);
        await Assert.That(relation.FollowerId).IsEqualTo(followerId);
        await Assert.That(relation.FolloweeId).IsEqualTo(followeeId);
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Accepted);
        await Assert.That(relation.DomainEvents.Count).IsEqualTo(0);
    }

    // --- Accept ---

    [Test]
    public async Task Accept_FromPending_TransitionsToAcceptedAndRecordsEvent()
    {
        var relation = CreatePendingFollow();

        var result = relation.Accept(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Accepted);
        await Assert.That(relation.DomainEvents.Count).IsEqualTo(2);

        var ev = (FollowAccepted)relation.DomainEvents[1];
        await Assert.That(ev.FollowId).IsEqualTo(relation.Id);
        await Assert.That(ev.OccurredAt).IsEqualTo(SampleOccurredAt);
    }

    [Test]
    public async Task Accept_WhenAlreadyAccepted_IsIdempotentAndDoesNotEmitEvent()
    {
        var relation = CreateAcceptedFollow();
        var countBefore = relation.DomainEvents.Count;

        var result = relation.Accept(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.DomainEvents.Count).IsEqualTo(countBefore);
    }

    [Test]
    [Arguments(FollowStatus.Rejected)]
    [Arguments(FollowStatus.Blocked)]
    [Arguments(FollowStatus.Removed)]
    public async Task Accept_FromInvalidStatus_ReturnsFailure(FollowStatus startStatus)
    {
        var (followerId, followeeId) = CreateActorPair();
        var relation = FollowRelation.Reconstitute(
            FollowId.New(),
            followerId,
            followeeId,
            startStatus,
            SampleCreatedAt
        );

        var result = relation.Accept(SampleOccurredAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(SocialGraphErrors.FollowTransitionInvalid);
        await Assert.That(relation.Status).IsEqualTo(startStatus);
    }

    // --- Reject ---

    [Test]
    public async Task Reject_FromPending_TransitionsToRejectedAndRecordsEvent()
    {
        var relation = CreatePendingFollow();

        var result = relation.Reject(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Rejected);

        var ev = (FollowRejected)relation.DomainEvents[1];
        await Assert.That(ev.FollowId).IsEqualTo(relation.Id);
    }

    [Test]
    public async Task Reject_FromAccepted_TransitionsToRejectedAndRecordsEvent()
    {
        var relation = CreateAcceptedFollow();

        var result = relation.Reject(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Rejected);
    }

    [Test]
    public async Task Reject_WhenAlreadyRejected_IsIdempotentAndDoesNotEmitEvent()
    {
        var relation = CreatePendingFollow();
        relation.Reject(SampleOccurredAt);
        var countBefore = relation.DomainEvents.Count;

        var result = relation.Reject(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.DomainEvents.Count).IsEqualTo(countBefore);
    }

    [Test]
    [Arguments(FollowStatus.Blocked)]
    [Arguments(FollowStatus.Removed)]
    public async Task Reject_FromInvalidStatus_ReturnsFailure(FollowStatus startStatus)
    {
        var (followerId, followeeId) = CreateActorPair();
        var relation = FollowRelation.Reconstitute(
            FollowId.New(),
            followerId,
            followeeId,
            startStatus,
            SampleCreatedAt
        );

        var result = relation.Reject(SampleOccurredAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(SocialGraphErrors.FollowTransitionInvalid);
    }

    // --- Unfollow ---

    [Test]
    public async Task Unfollow_FromAccepted_TransitionsToRemovedAndRecordsFollowRemovedEvent()
    {
        var relation = CreateAcceptedFollow();

        var result = relation.Unfollow(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Removed);

        var ev = (FollowRemoved)relation.DomainEvents[1];
        await Assert.That(ev.FollowId).IsEqualTo(relation.Id);
    }

    [Test]
    public async Task Unfollow_FromPending_TransitionsToRemoved()
    {
        var relation = CreatePendingFollow();

        var result = relation.Unfollow(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Removed);
    }

    [Test]
    public async Task Unfollow_WhenAlreadyRemoved_IsIdempotent()
    {
        var relation = CreateAcceptedFollow();
        relation.Unfollow(SampleOccurredAt);
        var countBefore = relation.DomainEvents.Count;

        var result = relation.Unfollow(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.DomainEvents.Count).IsEqualTo(countBefore);
    }

    [Test]
    public async Task Unfollow_WhenBlocked_ReturnsFailure()
    {
        var (followerId, followeeId) = CreateActorPair();
        var relation = FollowRelation.Reconstitute(
            FollowId.New(),
            followerId,
            followeeId,
            FollowStatus.Blocked,
            SampleCreatedAt
        );

        var result = relation.Unfollow(SampleOccurredAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(SocialGraphErrors.FollowTransitionInvalid);
    }

    // --- RemoveFollower ---

    [Test]
    public async Task RemoveFollower_FromAccepted_TransitionsToRemovedAndRecordsFollowerRemovedEvent()
    {
        var relation = CreateAcceptedFollow();

        var result = relation.RemoveFollower(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Removed);

        var ev = (FollowerRemoved)relation.DomainEvents[1];
        await Assert.That(ev.FollowId).IsEqualTo(relation.Id);
    }

    [Test]
    public async Task RemoveFollower_WhenAlreadyRemoved_IsIdempotent()
    {
        var relation = CreateAcceptedFollow();
        relation.RemoveFollower(SampleOccurredAt);
        var countBefore = relation.DomainEvents.Count;

        var result = relation.RemoveFollower(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.DomainEvents.Count).IsEqualTo(countBefore);
    }

    [Test]
    public async Task RemoveFollower_WhenBlocked_ReturnsFailure()
    {
        var (followerId, followeeId) = CreateActorPair();
        var relation = FollowRelation.Reconstitute(
            FollowId.New(),
            followerId,
            followeeId,
            FollowStatus.Blocked,
            SampleCreatedAt
        );

        var result = relation.RemoveFollower(SampleOccurredAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(SocialGraphErrors.FollowTransitionInvalid);
    }

    // --- Block ---

    [Test]
    public async Task Block_FromAccepted_TransitionsToBlockedAndRecordsEvent()
    {
        var relation = CreateAcceptedFollow();

        var result = relation.Block(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Blocked);

        var ev = (FollowBlocked)relation.DomainEvents[1];
        await Assert.That(ev.FollowId).IsEqualTo(relation.Id);
    }

    [Test]
    public async Task Block_FromPending_TransitionsToBlocked()
    {
        var relation = CreatePendingFollow();

        var result = relation.Block(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.Status).IsEqualTo(FollowStatus.Blocked);
    }

    [Test]
    public async Task Block_WhenAlreadyBlocked_IsIdempotent()
    {
        var relation = CreateAcceptedFollow();
        relation.Block(SampleOccurredAt);
        var countBefore = relation.DomainEvents.Count;

        var result = relation.Block(SampleOccurredAt);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(relation.DomainEvents.Count).IsEqualTo(countBefore);
    }

    [Test]
    public async Task Block_WhenRemoved_ReturnsFailure()
    {
        var (followerId, followeeId) = CreateActorPair();
        var relation = FollowRelation.Reconstitute(
            FollowId.New(),
            followerId,
            followeeId,
            FollowStatus.Removed,
            SampleCreatedAt
        );

        var result = relation.Block(SampleOccurredAt);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(SocialGraphErrors.FollowTransitionInvalid);
    }

    // --- Idempotency (same transition re-apply) ---

    [Test]
    public async Task AllIdempotentTransitions_DoNotCorruptState()
    {
        // Accepted -> Accept again (idempotent)
        var accepted = CreateAcceptedFollow();
        var r1 = accepted.Accept(SampleOccurredAt);
        await Assert.That(r1.IsSuccess).IsTrue();
        await Assert.That(accepted.Status).IsEqualTo(FollowStatus.Accepted);

        // Removed -> Unfollow again (idempotent)
        var removed = CreateAcceptedFollow();
        removed.Unfollow(SampleOccurredAt);
        var r2 = removed.Unfollow(SampleOccurredAt);
        await Assert.That(r2.IsSuccess).IsTrue();
        await Assert.That(removed.Status).IsEqualTo(FollowStatus.Removed);

        // Blocked -> Block again (idempotent)
        var blocked = CreateAcceptedFollow();
        blocked.Block(SampleOccurredAt);
        var r3 = blocked.Block(SampleOccurredAt);
        await Assert.That(r3.IsSuccess).IsTrue();
        await Assert.That(blocked.Status).IsEqualTo(FollowStatus.Blocked);
    }
}
