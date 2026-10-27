using Fediva.Domain.Common;
using Fediva.Domain.Common.Errors;

namespace Fediva.Domain.Identity.Actors;

/// <summary>
/// Actorのプロフィール情報を表す値オブジェクト
/// </summary>
public sealed record ActorProfile
{
    /// <summary>
    /// Bioの最大文字数(Fediverse仕様に合わせた500文字)
    /// </summary>
    public const int MaximumBioLength = 500;

    private ActorProfile(DisplayName displayName, string bio)
    {
        DisplayName = displayName;
        Bio = bio;
    }

    public DisplayName DisplayName { get; }

    public string Bio { get; }

    /// <summary>
    /// プロフィール情報を検証・生成します。
    /// </summary>
    /// <param name="displayName">表示名</param>
    /// <param name="bio">自己紹介文（任意）</param>
    /// <returns>生成結果</returns>
    public static Result<ActorProfile> Create(DisplayName displayName, string? bio = null)
    {
        ArgumentNullException.ThrowIfNull(displayName);

        var normalizedBio = bio?.Trim() ?? string.Empty;
        if (normalizedBio.Length > MaximumBioLength)
        {
            return Result<ActorProfile>.Failure(IdentityErrors.BioTooLong);
        }

        return Result<ActorProfile>.Success(new ActorProfile(displayName, normalizedBio));
    }

    public override string ToString() =>
        string.IsNullOrEmpty(Bio) ? DisplayName.ToString() : $"{DisplayName} ({Bio})";
}
