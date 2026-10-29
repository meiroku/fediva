namespace Fediva.Domain.Common.Errors;

public static class ContentErrors
{
    // Post関連エラー
    public static Error PostNotFound { get; } =
        new("content.post.not_found", "The requested post was not found.", ErrorType.NotFound);

    public static Error PostTextTooLong { get; } =
        new(
            "content.post.text_too_long",
            "The post text exceeds the maximum permitted length.",
            ErrorType.Validation
        );

    public static Error PostAlreadyDeleted { get; } =
        new(
            "content.post.already_deleted",
            "The post has already been deleted.",
            ErrorType.Conflict
        );

    public static Error PostNotEditable { get; } =
        new(
            "content.post.not_editable",
            "The post cannot be edited in its current state.",
            ErrorType.Conflict
        );

    public static Error PostEditorMismatch { get; } =
        new(
            "content.post.editor_mismatch",
            "Only the post author can edit or delete this post.",
            ErrorType.Forbidden
        );
}
