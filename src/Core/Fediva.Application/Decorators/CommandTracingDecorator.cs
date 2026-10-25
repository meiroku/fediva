using System.Diagnostics;
using Fediva.Application.Diagnostics;
using Fediva.Domain.Common;

namespace Fediva.Application.Decorators;

/// <summary>
/// 戻り値を持たないコマンドに分散トレーシングを付与するデコレーター
/// </summary>
/// <typeparam name="TCommand">処理対象のコマンドの型</typeparam>
public sealed class CommandTracingDecorator<TCommand>(ICommandHandler<TCommand> innerHandler)
    : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task<Result> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Application.StartActivity(
            $"Command: {typeof(TCommand).Name}"
        );
        activity?.SetTag("command.type", typeof(TCommand).FullName);

        try
        {
            var result = await innerHandler.HandleAsync(command, cancellationToken);
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

/// <summary>
/// 戻り値を持つコマンドに分散トレーシング (Activity) を付与するデコレーター
/// </summary>
/// <typeparam name="TCommand">処理対象のコマンドの型</typeparam>
/// <typeparam name="TResult">処理結果の値の型</typeparam>
public sealed class CommandTracingDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> innerHandler
) : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Application.StartActivity(
            $"Command: {typeof(TCommand).Name}"
        );
        activity?.SetTag("command.type", typeof(TCommand).FullName);

        try
        {
            var result = await innerHandler.HandleAsync(command, cancellationToken);
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
