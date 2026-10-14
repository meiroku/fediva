namespace Fediva.Domain.Common.Errors;

public sealed class DomainException : Exception
{
    public DomainException(Error error)
        : base(GetDescription(error))
    {
        Error = error;
    }

    public DomainException(Error error, Exception innerException)
        : base(
            GetDescription(error),
            innerException ?? throw new ArgumentNullException(nameof(innerException))
        )
    {
        Error = error;
    }

    public Error Error { get; }

    private static string GetDescription(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.IsNone)
        {
            throw new ArgumentException(
                "A domain exception cannot be created from Error.None.",
                nameof(error)
            );
        }

        return error.Description;
    }
}
