using System.Net.Mail;
using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;

namespace Fediva.Domain.Identity.Users;

public sealed record EmailAddress
{
    private const int MaximumLength = 254;

    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static Result<EmailAddress> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<EmailAddress>.Failure(IdentityErrors.EmailInvalid);
        }

        var normalizedValue = value.Trim();
        if (
            normalizedValue.Length > MaximumLength
            || !MailAddress.TryCreate(normalizedValue, out var parsedAddress)
            || !string.Equals(
                parsedAddress.Address,
                normalizedValue,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return Result<EmailAddress>.Failure(IdentityErrors.EmailInvalid);
        }

        return Result<EmailAddress>.Success(new EmailAddress(normalizedValue.ToLowerInvariant()));
    }

    public override string ToString() => Value;
}
