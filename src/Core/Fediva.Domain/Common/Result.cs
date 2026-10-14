using Fediva.Domain.Common.Errors;

namespace Fediva.Domain.Common;

public class Result
{
    private protected Result(bool isSuccess, Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException(
                "成功結果のErrorは、Error.Noneでなければなりません。",
                nameof(error)
            );
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException(
                "失敗結果には、Error.None以外のエラーが必要です。",
                nameof(error)
            );
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                $"失敗結果からValueは取得できません。エラー: {Error}"
            );

    public static Result<T> Success(T value) => new(value, true, Error.None);

    public static new Result<T> Failure(Error error) => new(default, false, error);
}
