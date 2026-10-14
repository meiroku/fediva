using System.Collections.ObjectModel;

namespace Fediva.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];
    private readonly ReadOnlyCollection<DomainEvent> _readOnlyDomainEvents;

    protected AggregateRoot(TId id)
        : base(id)
    {
        _readOnlyDomainEvents = _domainEvents.AsReadOnly();
    }

    public IReadOnlyList<DomainEvent> DomainEvents => _readOnlyDomainEvents;

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
