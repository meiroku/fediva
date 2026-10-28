using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;

namespace Fediva.Domain.Content;

/// <summary>
/// Post本文を表すValue Object
/// HTMLへの変換はAdapter(HtmlToTextConverter / TextToHtmlConverter)の責務
/// </summary>
public sealed record PostContent
{
    /// <summary>プレーンテキストとして扱う本文の最大長</summary>
    public const int MaxLength = 3000;

    private PostContent(string text)
    {
        Text = text;
    }

    /// <summary>プレーンテキスト本文(空文字列は添付のみ投稿を許容するために許可)</summary>
    public string Text { get; }

    public static Result<PostContent> Create(string? text)
    {
        var normalized = text?.Trim() ?? string.Empty;

        if (normalized.Length > MaxLength)
        {
            return Result<PostContent>.Failure(ContentErrors.PostTextTooLong);
        }

        return Result<PostContent>.Success(new PostContent(normalized));
    }
}
