namespace Fediva.Domain.Common;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
        : this(Guid.NewGuid(), DateTimeOffset.UtcNow) { }

    protected DomainEvent(Guid eventId, DateTimeOffset occurredOn)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException(
                "イベントIDにGuid.Emptyは指定できません。",
                nameof(eventId)
            );
        }

        if (occurredOn == default)
        {
            throw new ArgumentOutOfRangeException(
                nameof(occurredOn),
                occurredOn,
                "イベント発生日時を指定してください。"
            );
        }

        EventId = eventId;
        OccurredOn = occurredOn.ToUniversalTime();
    }

    public Guid EventId { get; }

    public DateTimeOffset OccurredOn { get; }
}
