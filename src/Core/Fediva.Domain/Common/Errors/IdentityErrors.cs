namespace Fediva.Domain.Common.Errors;

public static class IdentityErrors
{
    public static Error UserNotFound { get; } =
        new("identity.user.not_found", "The required user is not found.", ErrorType.NotFound);

    public static Error UserStatusTransitionInvalid { get; } =
        new(
            "identity.user.status_transition_invalid",
            "The user status cannot be changed from a terminal state.",
            ErrorType.Conflict
        );

    /// <summary>
    /// Usernameが指定されていない。
    /// </summary>
    public static Error UsernameRequired { get; } =
        new("identity.username.required", "A username is required.", ErrorType.Validation);

    public static Error UsernameInvalid { get; } =
        new("identity.username.invalid", "The username is invalid.", ErrorType.Validation);

    public static Error UsernameAlreadyExists { get; } =
        new(
            "identity.username.already_exists",
            "The username is already in use.",
            ErrorType.Conflict
        );

    public static Error DisplayNameTooLong { get; } =
        new(
            "identity.display_name.too_long",
            "The display name exceeds the maximum permitted length.",
            ErrorType.Validation
        );

    public static Error EmailInvalid { get; } =
        new("identity.email.invalid", "The email address is invalid.", ErrorType.Validation);

    /// <summary>
    /// メールアドレスがすでに存在する。
    /// </summary>
    public static Error EmailAlreadyExists { get; } =
        new(
            "identity.email.already_exists",
            "The email address is already associated with an identity.",
            ErrorType.Conflict
        );

    /// <summary>
    /// 外部のIdentityがすでにリンクされている。
    /// </summary>
    public static Error ExternalIdentityAlreadyLinked { get; } =
        new(
            "identity.external_identity.already_linked",
            "The external identity is already linked.",
            ErrorType.Conflict
        );

    /// <summary>
    /// リクエストされた外部Identityが存在しない。
    /// </summary>
    public static Error ExternalIdentityNotFound { get; } =
        new(
            "identity.external_identity.not_found",
            "The requested external identity was not found.",
            ErrorType.NotFound
        );
}
