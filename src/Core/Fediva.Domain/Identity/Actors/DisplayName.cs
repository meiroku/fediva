using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;

namespace Fediva.Domain.Identity.Actors;

/// <summary>
/// Actorの表示名を表す値オブジェクト
/// </summary>
public sealed record DisplayName
{
    /// <summary>
    /// 表示名の最大文字数(ActivityPub等のFederation仕様との互換性を考慮した50文字）
    /// </summary>
    public const int MaximumLength = 50;

    private DisplayName(string value) => Value = value;

    public string Value { get; }

    /// <summary>
    /// 表示名を検証・正規化して生成します。
    /// </summary>
    /// <param name="value">表示名の文字列</param>
    /// <returns>生成結果</returns>
    public static Result<DisplayName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<DisplayName>.Failure(IdentityErrors.DisplayNameRequired);
        }

        var normalizedValue = value.Trim();
        if (normalizedValue.Length > MaximumLength)
        {
            return Result<DisplayName>.Failure(IdentityErrors.DisplayNameTooLong);
        }

        return Result<DisplayName>.Success(new DisplayName(normalizedValue));
    }

    public override string ToString() => Value;
}
