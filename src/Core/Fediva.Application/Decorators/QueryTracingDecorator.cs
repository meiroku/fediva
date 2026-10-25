using System.Diagnostics;
using Fediva.Application.Diagnostics;
using Fediva.Domain.Common;

namespace Fediva.Application.Decorators;

/// <summary>
/// クエリに分散トレーシングを付与するデコレーター
/// </summary>
/// <typeparam name="TQuery">処理対象のクエリの型</typeparam>
/// <typeparam name="TResult">処理結果の値の型</typeparam>
public sealed class QueryTracingDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> innerHandler
) : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public async Task<Result<TResult>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        using var activity = ActivitySources.Application.StartActivity(
            $"Query: {typeof(TQuery).Name}"
        );
        activity?.SetTag("query.type", typeof(TQuery).FullName);

        try
        {
            var result = await innerHandler.HandleAsync(query, cancellationToken);
            if (result.IsFailure)
            {
                activity?.SetStatus(ActivityStatusCode.Error, result.Error.Description);
                activity?.SetTag("error.code", result.Error.Code);
            }
            else
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
            }

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }
}
