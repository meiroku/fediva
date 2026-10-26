namespace Fediva.Domain.Identity.Actors;

public readonly record struct ActorId : IComparable<ActorId>
{
    public ActorId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Actor IDにGuid.Emptyは指定できません。", nameof(value));
        }

        var bytes = value.ToByteArray(bigEndian: true);
        if ((bytes[6] & 0xF0) != 0x70)
        {
            throw new ArgumentException("Actor IDにはUUIDv7を指定してください。", nameof(value));
        }

        Value = value;
    }

    public static ActorId New() => new(Guid.CreateVersion7());

    public Guid Value { get; }

    public int CompareTo(ActorId other) => Value.CompareTo(other.Value);
}
