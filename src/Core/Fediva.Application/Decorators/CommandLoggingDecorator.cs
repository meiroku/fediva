using System.Diagnostics;
using Fediva.Application.Diagnostics;
using Fediva.Domain.Common;

namespace Fediva.Application.Decorators;

/// <summary>
/// 戻り値を持たないコマンドにロギングを付与するデコレーター
/// </summary>
/// <typeparam name="TCommand">処理対象のコマンドの型</typeparam>
public sealed class CommandLoggingDecorator<TCommand>(
    ICommandHandler<TCommand> innerHandler,
    IAppLogger<CommandLoggingDecorator<TCommand>> logger
) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task<Result> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandName = typeof(TCommand).Name;
        logger.LogInformation("Executing command {CommandName}", commandName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await innerHandler.HandleAsync(command, cancellationToken);
            stopwatch.Stop();

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "Command {CommandName} failed with error {ErrorCode}: {ErrorDescription} in {ElapsedMilliseconds}ms",
                    commandName,
                    result.Error.Code,
                    result.Error.Description,
                    stopwatch.ElapsedMilliseconds
                );
            }
            else
            {
                logger.LogInformation(
                    "Command {CommandName} succeeded in {ElapsedMilliseconds}ms",
                    commandName,
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
                "Command {CommandName} threw an unhandled exception in {ElapsedMilliseconds}ms",
                commandName,
                stopwatch.ElapsedMilliseconds
            );
            throw;
        }
    }
}

/// <summary>
/// 戻り値を持つコマンドにロギングを付与するデコレーター
/// </summary>
/// <typeparam name="TCommand">処理対象のコマンドの型</typeparam>
/// <typeparam name="TResult">処理結果の値の型</typeparam>
public sealed class CommandLoggingDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> innerHandler,
    IAppLogger<CommandLoggingDecorator<TCommand, TResult>> logger
) : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandName = typeof(TCommand).Name;
        logger.LogInformation("Executing command {CommandName}", commandName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await innerHandler.HandleAsync(command, cancellationToken);
            stopwatch.Stop();

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "Command {CommandName} failed with error {ErrorCode}: {ErrorDescription} in {ElapsedMilliseconds}ms",
                    commandName,
                    result.Error.Code,
                    result.Error.Description,
                    stopwatch.ElapsedMilliseconds
                );
            }
            else
            {
                logger.LogInformation(
                    "Command {CommandName} succeeded in {ElapsedMilliseconds}ms",
                    commandName,
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
                "Command {CommandName} threw an unhandled exception in {ElapsedMilliseconds}ms",
                commandName,
                stopwatch.ElapsedMilliseconds
            );
            throw;
        }
    }
}
