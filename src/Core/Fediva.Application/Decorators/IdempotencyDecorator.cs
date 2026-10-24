using Fediva.Domain.Common;

namespace Fediva.Application.Decorators;

/// <summary>
/// 戻り値を持たないコマンドに冪等性保証を付与するデコレーター
/// </summary>
/// <typeparam name="TCommand">処理対象のコマンドの型</typeparam>
public sealed class IdempotencyDecorator<TCommand>(
    ICommandHandler<TCommand> innerHandler,
    IIdempotencyStore idempotencyStore
) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task<Result> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command is IIdempotentCommand idempotentCommand)
        {
            var key = idempotentCommand.IdempotencyKey;
            if (await idempotencyStore.HasBeenProcessedAsync(key, cancellationToken))
            {
                return Result.Success();
            }

            var result = await innerHandler.HandleAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                await idempotencyStore.MarkAsProcessedAsync(key, cancellationToken);
            }

            return result;
        }

        return await innerHandler.HandleAsync(command, cancellationToken);
    }
}

/// <summary>
/// 戻り値を持つコマンドに冪等性保証を付与するデコレーター
/// </summary>
/// <typeparam name="TCommand">処理対象のコマンドの型</typeparam>
/// <typeparam name="TResult">処理結果の値の型</typeparam>
public sealed class IdempotencyDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> innerHandler,
    IIdempotencyStore idempotencyStore
) : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command is IIdempotentCommand<TResult> idempotentCommand)
        {
            var key = idempotentCommand.IdempotencyKey;
            if (await idempotencyStore.HasBeenProcessedAsync(key, cancellationToken))
            {
                return Result<TResult>.Success(default!);
            }

            var result = await innerHandler.HandleAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                await idempotencyStore.MarkAsProcessedAsync(key, cancellationToken);
            }

            return result;
        }

        return await innerHandler.HandleAsync(command, cancellationToken);
    }
}
