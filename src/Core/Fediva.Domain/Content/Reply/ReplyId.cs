namespace Fediva.Domain.Content;

public readonly record struct ReplyId : IComparable<ReplyId>
{
    public ReplyId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Reply IDにGuid.Emptyは指定できません。", nameof(value));
        }

        var bytes = value.ToByteArray(bigEndian: true);
        if ((bytes[6] & 0xF0) != 0x70)
        {
            throw new ArgumentException("Reply IDにはUUIDv7を指定してください。", nameof(value));
        }

        Value = value;
    }

    public static ReplyId New() => new(Guid.CreateVersion7());

    public Guid Value { get; }

    public int CompareTo(ReplyId other) => Value.CompareTo(other.Value);
}
