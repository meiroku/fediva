namespace Fediva.Domain.Content;

public readonly record struct PostId : IComparable<PostId>
{
    public PostId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Post IDにGuid.Emptyは指定できません。", nameof(value));
        }

        var bytes = value.ToByteArray(bigEndian: true);
        if ((bytes[6] & 0xF0) != 0x70)
        {
            throw new ArgumentException("Post IDにはUUIDv7を指定してください。", nameof(value));
        }

        Value = value;
    }

    public static PostId New() => new(Guid.CreateVersion7());

    public Guid Value { get; }

    public int CompareTo(PostId other) => Value.CompareTo(other.Value);
}
