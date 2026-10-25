using Fediva.Application.Ports.Persistence;
using Fediva.Domain.Common;

namespace Fediva.Application.Decorators;

/// <summary>
/// 戻り値を持たないコマンドにトランザクションと作業単位の永続化コミットを付与するデコレーター
/// </summary>
/// <typeparam name="TCommand">処理対象のコマンドの型</typeparam>
public sealed class TransactionDecorator<TCommand>(
    ICommandHandler<TCommand> innerHandler,
    IUnitOfWork unitOfWork
) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task<Result> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await innerHandler.HandleAsync(command, cancellationToken);
            if (result.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return result;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

/// <summary>
/// 戻り値を持つコマンドにトランザクションと作業単位の永続化コミットを付与するデコレーター
/// </summary>
/// <typeparam name="TCommand">処理対象のコマンドの型</typeparam>
/// <typeparam name="TResult">処理結果の値の型</typeparam>
public sealed class TransactionDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> innerHandler,
    IUnitOfWork unitOfWork
) : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await innerHandler.HandleAsync(command, cancellationToken);
            if (result.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return result;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
