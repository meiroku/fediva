namespace Fediva.Domain.Content;

public readonly record struct ReactionId : IComparable<ReactionId>
{
    public ReactionId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Reaction IDにGuid.Emptyは指定できません。", nameof(value));
        }

        var bytes = value.ToByteArray(bigEndian: true);
        if ((bytes[6] & 0xF0) != 0x70)
        {
            throw new ArgumentException("Reaction IDにはUUIDv7を指定してください。", nameof(value));
        }

        Value = value;
    }

    public static ReactionId New() => new(Guid.CreateVersion7());

    public Guid Value { get; }

    public int CompareTo(ReactionId other) => Value.CompareTo(other.Value);
}
