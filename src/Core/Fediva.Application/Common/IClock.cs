namespace Fediva.Application.Common;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
