namespace Fediva.Domain.Identity.Users;

public readonly record struct UserId : IComparable<UserId>
{
    public UserId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ユーザーIDにGuid.Emptyは指定できません。", nameof(value));
        }

        var bytes = value.ToByteArray(bigEndian: true);
        if ((bytes[6] & 0xF0) != 0x70)
        {
            throw new ArgumentException("ユーザーIDにはUUIDv7を指定してください。", nameof(value));
        }

        Value = value;
    }

    public static UserId New() => new(Guid.CreateVersion7());

    public Guid Value { get; }

    public int CompareTo(UserId other) => Value.CompareTo(other.Value);
}
