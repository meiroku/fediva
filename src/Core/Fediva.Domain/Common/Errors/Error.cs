namespace Fediva.Domain.Common.Errors;

public sealed record Error
{
    public static Error None { get; } = new();

    /// <summary>
    /// エラーがないことを示すコンストラクタ
    /// </summary>
    private Error()
    {
        Code = string.Empty;
        Description = string.Empty;
        Type = ErrorType.None;
    }

    /// <summary>
    /// ドメインエラーを初期化
    /// </summary>
    /// <param name="code">機械が読めるエラーコード</param>
    /// <param name="description">人間が読むエラーの説明</param>
    /// <param name="type">エラーのカテゴリ</param>
    public Error(string code, string description, ErrorType type = ErrorType.Failure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (type == ErrorType.None || !Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "A domain error must have a valid, non-None error type."
            );
        }

        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }

    public string Description { get; }

    public ErrorType Type { get; }

    /// <summary>
    /// このインスタンスがエラーのない状態を表しているかどうかを取得
    /// </summary>
    public bool IsNone => Type == ErrorType.None;
}
