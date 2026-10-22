using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;

namespace Fediva.Domain.Identity.Users;

public sealed record DisplayName
{
    private const int MaximumLength = 30;

    private DisplayName(string value) => Value = value;

    public string Value { get; }

    public static Result<DisplayName> Create(string? value)
    {
        var normalizedValue = value?.Trim() ?? string.Empty;
        if (normalizedValue.Length > MaximumLength)
        {
            return Result<DisplayName>.Failure(IdentityErrors.DisplayNameTooLong);
        }

        return Result<DisplayName>.Success(new DisplayName(normalizedValue));
    }

    public override string ToString() => Value;
}
