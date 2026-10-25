using System.Diagnostics;
using Fediva.Application.Diagnostics;
using Fediva.Domain.Common;

namespace Fediva.Application.Decorators;

/// <summary>
/// クエリにロギングを付与するデコレーター
/// </summary>
/// <typeparam name="TQuery">処理対象のクエリの型</typeparam>
/// <typeparam name="TResult">処理結果の値の型</typeparam>
public sealed class QueryLoggingDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> innerHandler,
    IAppLogger<QueryLoggingDecorator<TQuery, TResult>> logger
) : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public async Task<Result<TResult>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryName = typeof(TQuery).Name;
        logger.LogInformation("Executing query {QueryName}", queryName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await innerHandler.HandleAsync(query, cancellationToken);
            stopwatch.Stop();

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "Query {QueryName} failed with error {ErrorCode}: {ErrorDescription} in {ElapsedMilliseconds}ms",
                    queryName,
                    result.Error.Code,
                    result.Error.Description,
                    stopwatch.ElapsedMilliseconds
                );
            }
            else
            {
                logger.LogInformation(
                    "Query {QueryName} succeeded in {ElapsedMilliseconds}ms",
                    queryName,
                    stopwatch.ElapsedMilliseconds
                );
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "Query {QueryName} threw an unhandled exception in {ElapsedMilliseconds}ms",
                queryName,
                stopwatch.ElapsedMilliseconds
            );
            throw;
        }
    }
}
