namespace Fediva.Domain.Common.Errors;

public static class SocialGraphErrors
{
    public static Error FollowRelationNotFound { get; } =
        new(
            "social_graph.follow.not_found",
            "The follow relation was not found.",
            ErrorType.NotFound
        );

    public static Error SelfFollowNotAllowed { get; } =
        new(
            "social_graph.follow.self_follow_not_allowed",
            "An actor cannot follow itself.",
            ErrorType.Validation
        );

    public static Error FollowTransitionInvalid { get; } =
        new(
            "social_graph.follow.transition_invalid",
            "The follow status transition is not permitted.",
            ErrorType.Conflict
        );
}
