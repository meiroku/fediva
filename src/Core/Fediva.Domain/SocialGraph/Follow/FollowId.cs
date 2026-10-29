namespace Fediva.Domain.SocialGraph;

public readonly record struct FollowId : IComparable<FollowId>
{
    public FollowId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Follow IDにGuid.Emptyは指定できません。", nameof(value));
        }

        var bytes = value.ToByteArray(bigEndian: true);
        if ((bytes[6] & 0xF0) != 0x70)
        {
            throw new ArgumentException("Follow IDにはUUIDv7を指定してください。", nameof(value));
        }

        Value = value;
    }

    public static FollowId New() => new(Guid.CreateVersion7());

    public Guid Value { get; }

    public int CompareTo(FollowId other) => Value.CompareTo(other.Value);
}
